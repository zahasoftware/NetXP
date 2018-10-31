using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NetXP.NetStandard.Network.LittleJsonProtocol;
using NetXP.NetStandard.Compression;
using NetXP.NetStandard.Network.TCP;
using NetXP.NetStandard;
using Microsoft.Extensions.Options;
using NetXP.NetStandard.Factories;
using NetXP.NetStandard.Auditory;

namespace NetXP.NetStandard.Network.SecureLittleProtocol.Implementations
{
    public class SLPClientConnector : IClientConnector
    {
        public SLPClientConnector(
              INameResolverFactory<IAsymetricCrypt> asymetricCryptFactory,
              ISymetricCrypt symetric,
              ISerializerFactory serializeT,
              ILogger logger,
              IHash hash,
              IPersistentPrivateKeyProvider persistentPrivateKeyProvider,
              ICompression compression,
              ISecureProtocolHandshake secureProtocolHandshake,
              IClientConnector clientConnector,
              IOptions<SLJPOption> sljpOptions = null
        )
        {
            this.asymetricForDecrypt = asymetricCryptFactory.Resolve();
            this.asymetricToEncrypt = asymetricCryptFactory.Resolve();
            this.firstAsymetricHandshakeToDecrypt = asymetricCryptFactory.Resolve();
            this.firstAsymetricHandshakeToEncrypt = asymetricCryptFactory.Resolve();

            this.textPlainTCPChannel = clientConnector;

            this.symetric = symetric;
            this.serializeT = serializeT.Resolve(SerializerType.Json);
            this.logger = logger;
            this.hash = hash;
            this.IPersistentPrivateKeyProvider = persistentPrivateKeyProvider;
            this.compression = compression;
            this.secureProtocolHandshake = secureProtocolHandshake;
            var SecurityMaxSizeToReceive = sljpOptions.Value.SecurityMaxSizeToReceive;
            this.sljpOptions = sljpOptions.Value ?? new SLJPOption();
        }

        public bool IsConnected { get { return this.textPlainTCPChannel.IsConnected; } }

        //To Reduce New
        private byte[] headerType3Buffer = new byte[HEADER_TYPE_3_SIZE];
        private byte[] headerType2Buffer = new byte[HEADER_TYPE_2_SIZE];
        private byte[] headerType1Buffer = new byte[HEADER_TYPE_1_SIZE];

        private byte[] aLittleBuffer = new byte[LITTLE_BUFFER_SIZE];
        private byte[] aMidleBuffer = new byte[MIDLE_BUFFER_SIZE];
        private byte[] aBigBuffer = new byte[BIG_BUFFER_SIZE];

        private byte[] aDecryptedMessage;
        private int iReceiveRemaining;

        public void Connect(IPAddress ipAddress, int port)
        {
            this.textPlainTCPChannel.Connect(ipAddress, port);

            //Read a persistent private key or create a new

            //TODO: See for domain name server rather than ip.
            var ppk = this.IPersistentPrivateKeyProvider.Read(ipAddress.ToString(), port);
            PublicKey publicKeyToSend = null;

            if (ppk == null)//New key to generate in server.
            {
                this.asymetricForDecrypt.GenerateKeys();
                this.thisPrivateKey = this.asymetricForDecrypt.GetPrivateKey();//To Decrypt Server Messages
                publicKeyToSend = asymetricForDecrypt.GetPublicKey();
            }
            else //Using existing key.
            {
                publicKeyToSend = new PublicKey { yExponent = ppk.PrivateKey.yExponent, yModulus = ppk.PrivateKey.yModulus };
                this.thisPrivateKey = ppk.PrivateKey;
            }

            bool IsKeyFound = ppk != null;
            SendPublicKey(publicKeyToSend, IsKeyFound, false);

            //Should receive the public key 
            this.Receive(aLittleBuffer, 0, aLittleBuffer.Length);

            //if (ppk == null)//No save yet
            {
                PersistentPrivateKey ppkNew = new PersistentPrivateKey
                {
                    Port = port,
                    Server = ipAddress.ToString(),
                    PrivateKey = this.thisPrivateKey,
                    PublicKeyRemote = this.remotePublicKey,
                    FromServer = false
                };
                IPersistentPrivateKeyProvider.Save(ppkNew);
            }
        }

        public void SendPublicKey(PublicKey publicKey, bool isPpkFound, bool isFromServer)
        {
            var bytesHeader = new byte[HEADER_TYPE_3_SIZE];
            bytesHeader[HEADER_MAJOR_OFFSET] = 1;
            bytesHeader[HEADER_MINOR_OFFSET] = 0;

            bytesHeader[HEADER_FLAGS_OFFSET] |= (isPpkFound ? FLAGS_PPK_FOUND : (byte)0);
            bytesHeader[HEADER_FLAGS_OFFSET] |= (isFromServer ? FLAGS_SERVER_REQUEST : (byte)0);
            bytesHeader[HEADER_TYPE_OFFSET] = TYPE_FIRST_HANDSHAKE;

            //Making Symetric Key.
            var symetricKey = this.symetric.Generate();
            var symetricKeySerialized = this.serializeT.Serialize(symetricKey);

            var firstPublicKeyToEncrypt = secureProtocolHandshake.GetFirstPublickKey();
            this.firstAsymetricHandshakeToEncrypt.SetPublicKey(firstPublicKeyToEncrypt);

            var symetricKeyEncrypted = firstAsymetricHandshakeToEncrypt.Encrypt(symetricKeySerialized);
            ByteHelper.UIntToByte(bytesHeader, HEADER_TYPE_3_SYMKEY_LENGTH_OFFSET, (uint)symetricKeyEncrypted.Length);

            //Making Body
            var publicKeySerialized = serializeT.Serialize(publicKey);
            var publicKeyCompressed = this.compression.Compress(new ByteArray(publicKeySerialized.Take(publicKeySerialized.Length).ToArray()));
            var bodyEncrypted = this.symetric.Encrypt(publicKeyCompressed, symetricKey);
            ByteHelper.UIntToByte(bytesHeader, HEADER_TYPE_3_BODY_LENGTH_OFFSET, (uint)bodyEncrypted.Length);

            var bytesToSend = bytesHeader.Concat(symetricKeyEncrypted).Concat(bodyEncrypted).ToArray();

#if DEBUG
            var publicKeyHash = hash.Generate(new ByteArray(publicKeySerialized));
            logger.Debug($"Sending PK (FromServer={isFromServer},Founded={isPpkFound}) \"{BitConverter.ToString(publicKeyHash)}\"");
#endif
            textPlainTCPChannel.Send(bytesToSend, 0, bytesToSend.Length);
        }

        public void Disconnect(bool dispose = true)
        {
            this.textPlainTCPChannel.Disconnect(dispose);
        }

        private void ReceiveAll(byte[] aMessage, int iLength)
        {
            int iReceived = 0;
            while (iReceived < iLength)
            {
                iReceived += this.textPlainTCPChannel.Receive(aMessage, iReceived, iLength - iReceived);
                if (iReceived <= 0)
                    break;
            }
        }

        public int Receive(byte[] aInputBuffer, int iOffset, int iLength)
        {
            //If the class has not yet received all the information
            if (iReceiveRemaining != 0)
            {
                Buffer.BlockCopy(this.aDecryptedMessage, this.aDecryptedMessage.Length - iReceiveRemaining, aInputBuffer, iOffset, Math.Min(iReceiveRemaining, iLength));

                if (iLength < iReceiveRemaining)
                {
                    iReceiveRemaining -= iLength;
                    return iLength;
                }
                else
                {
                    iReceiveRemaining = 0;
                    this.aDecryptedMessage = null;
                    return iLength - iReceiveRemaining;
                }
            }

            //Receiving header and part of encrypted message
            Array.Clear(aLittleBuffer, 0, aLittleBuffer.Length);
            var iReceivedBytes = this.textPlainTCPChannel.Receive(aLittleBuffer, 0, HEADER_TYPE_3_SIZE);

            if (iReceivedBytes <= 0)
            {
                throw new SLPException($"Not data received, Error code: {iReceivedBytes}.", SLPException.SLPExceptionType.NoDataTimeOut);
            }

            byte yMajor = aLittleBuffer[HEADER_MAJOR_OFFSET];//Extract Major
            byte yMinor = aLittleBuffer[HEADER_MINOR_OFFSET];//Extract Minor

            if (yMajor != 1 || yMinor != 0)
            {
                throw new SLPException("Receive:Invalid versión or data.", SLPException.SLPExceptionType.BadProtocol);
            }
            else
            {
                byte flags = aLittleBuffer[HEADER_FLAGS_OFFSET];//Extract Options
                byte yType = aLittleBuffer[HEADER_TYPE_OFFSET];//Extract Type

                if (!(new List<byte>() { 1, 2, 3 }.Contains(yType)))
                {
                    throw new SLPException("Receive:Invalid type.", SLPException.SLPExceptionType.BadProtocol);
                }

                if (yType == TYPE_FIRST_HANDSHAKE)//(Used In Connection And Accept as part of protocol SLJP)
                {
                    return HandShakeStep(yMajor, yMinor, flags);
                }
                else
                {
                    ReceivingEncryptedMessage(this.asymetricForDecrypt);

                    Buffer.BlockCopy(aDecryptedMessage, 0, aInputBuffer, iOffset, Math.Min(aDecryptedMessage.Length, iLength));

                    if (iLength < aDecryptedMessage.Length)
                    {
                        iReceiveRemaining = aDecryptedMessage.Length - iLength;
                        return iLength;
                    }
                    else
                    {
                        return aDecryptedMessage.Length;
                    }
                    //COPY BLOCK
                }
            }
        }

        private void ReceivingEncryptedMessage(IAsymetricCrypt asymetricCrypt)
        {
            //Receiving the rest of header.
            int iSymetricLength = BitHelper.ToInt32(aLittleBuffer, HEADER_TYPE_3_SYMKEY_LENGTH_OFFSET);//Length of message
            int iBodyLength = BitHelper.ToInt32(aLittleBuffer, HEADER_TYPE_3_BODY_LENGTH_OFFSET);

            ValidatingMaxOfSizeToReceive(iSymetricLength, "Symetric Key Length");
            ValidatingMaxOfSizeToReceive(iBodyLength, "Body Length");

            //Saving encrypted symetric key in little buffer.
            this.ReceiveAll(aLittleBuffer, iSymetricLength);

            //Saving encrypted body in dynamic buffer.
            var aDinamicBuffer = new byte[iBodyLength];//Ugly new if is big data 
            this.ReceiveAll(aDinamicBuffer, iBodyLength);

            #region For Debuging
#if DEBUG
            byte[] header = new byte[4 + 4 + 4];
            header[HEADER_TYPE_OFFSET] = TYPE_MESSAGE;
            ByteHelper.UIntToByte(header, HEADER_TYPE_3_SYMKEY_LENGTH_OFFSET, (uint)iSymetricLength);
            ByteHelper.UIntToByte(header, HEADER_TYPE_3_BODY_LENGTH_OFFSET, (uint)iBodyLength);
            var aReceivedHash = hash.Generate(new ByteArray(header.Concat(aLittleBuffer.Take(iSymetricLength).ToArray()).Concat(aDinamicBuffer).ToArray()));
            this.logger.Debug($"Receiving Message Hash \"{BitConverter.ToString(aReceivedHash)}\", BodyLenth={iBodyLength}");

            var aPUBKEYToDecrypt = this.hash.Generate(new ByteArray(this.serializeT.Serialize(asymetricCrypt.GetPublicKey())));
            this.logger.Debug($"PUBKEY to decrypt the message {BitConverter.ToString(aPUBKEYToDecrypt)}");
#endif
            #endregion

            //SymetricKey Decrypting and deserialization 
            byte[] aSymetricKey = asymetricCrypt.Decrypt(aLittleBuffer.Take(iSymetricLength).ToArray());
            SymetricKey symetricKey = this.serializeT.Deserialize<SymetricKey>(aSymetricKey);

            //Decrypting and Decrompres and deserialization of Body with symetric key
            var aDecryptedMessageCompressed = this.symetric.Decrypt(aDinamicBuffer.Take(iBodyLength).ToArray(), symetricKey);
            aDecryptedMessage = this.compression.Decrompress(new ByteArray(aDecryptedMessageCompressed));
            aDinamicBuffer = null;
        }

        private void ValidatingMaxOfSizeToReceive(int iReceivedBytes, string v)
        {
            if (iReceivedBytes > this.sljpOptions.MaxOfBytesToReceive)
            {
                throw new SLPException($"Max size to receive when trying \"{v}\"", SLPException.SLPExceptionType.MaxSizeToReceive);
            }
        }

        private int HandShakeStep(byte yMajor, byte yMinor, byte flags)
        {
            ///Using first Private Key (Application Level)
            this.firstAsymetricHandshakeToDecrypt
                .SetPrivateKey(
                                secureProtocolHandshake.GetFirstPrivateKey()
                               );

            ReceivingEncryptedMessage(this.firstAsymetricHandshakeToDecrypt);
            this.remotePublicKey = serializeT.Deserialize<PublicKey>(aDecryptedMessage);

            ///[ServerHandshake turn]Receiving serialize public key 
            if (
                   FLAGS_PPK_FOUND == (FLAGS_PPK_FOUND & flags)
                && FLAGS_SERVER_REQUEST != (FLAGS_SERVER_REQUEST & flags))//Search for a ppk in disk
            {
                var ppk = this.IPersistentPrivateKeyProvider.Read(this.remotePublicKey);
                if (ppk == null)
                {
                    throw new SLPException("Secure protocol, ppk not found,  ppk file could have been deleted.", SLPException.SLPExceptionType.PPKNotFound);//TODO: From configuration.
                }

                PublicKey PubKey = new PublicKey { yExponent = ppk.PrivateKey.yExponent, yModulus = ppk.PrivateKey.yModulus };
                SendPublicKey(PubKey, true, true);

                this.remotePublicKey = ppk.PublicKeyRemote;
                this.thisPrivateKey = ppk.PrivateKey;

                this.IPersistentPrivateKeyProvider.Save(ppk);//To prolong expiration date
            }
            ///[ServerHandshake turn] Client requests a new public key.
            else if (
                   FLAGS_PPK_FOUND != (FLAGS_PPK_FOUND & flags)
                && FLAGS_SERVER_REQUEST != (FLAGS_SERVER_REQUEST & flags))
            {
                this.asymetricForDecrypt.GenerateKeys();
                this.thisPrivateKey = this.asymetricForDecrypt.GetPrivateKey();
#if DEBUG
                var aPUBKEYToDecrypt = this.hash.Generate(new ByteArray(this.serializeT.Serialize(this.asymetricForDecrypt.GetPublicKey())));
                this.logger.Debug($"Saving a New PrivateKey, PUBKEY (HASH) is = {BitConverter.ToString(aPUBKEYToDecrypt)}");
#endif
                var ppk = new PersistentPrivateKey
                {
                    PrivateKey = this.thisPrivateKey,
                    PublicKeyRemote = this.remotePublicKey,
                    FromServer = true
                };
                this.IPersistentPrivateKeyProvider.Save(ppk);

                PublicKey PubKey = new PublicKey
                {
                    yExponent = ppk.PrivateKey.yExponent,
                    yModulus = ppk.PrivateKey.yModulus,
                };
                this.SendPublicKey(PubKey, false, true);
            }

            return -1000;//Returning any negative integer
        }

        public int Send(byte[] aOutputBuffer, int iOffset, int iLength)
        {
            //Making Header
            var aHeader = new byte[HEADER_TYPE_3_SIZE];
            aHeader[HEADER_MAJOR_OFFSET] = 1;
            aHeader[HEADER_MINOR_OFFSET] = 0;
            aHeader[HEADER_FLAGS_OFFSET] = 0;
            aHeader[HEADER_TYPE_OFFSET] = TYPE_MESSAGE;

            //Making Symetric Key.
            var symetricKey = this.symetric.Generate();
            var aSymetricKeySerialized = this.serializeT.Serialize(symetricKey);

            var aSymetricKeyEncrypted = this.asymetricToEncrypt.Encrypt(aSymetricKeySerialized);
            ByteHelper.UIntToByte(aHeader, HEADER_TYPE_3_SYMKEY_LENGTH_OFFSET, (uint)aSymetricKeyEncrypted.Length);

            //Making Body
            var aOutputBufferCompressed = this.compression.Compress(new ByteArray(aOutputBuffer.Skip(iOffset).Take(iLength).ToArray()));
            var aBodyEncrypted = this.symetric.Encrypt(aOutputBufferCompressed, symetricKey);
            ByteHelper.UIntToByte(aHeader, HEADER_TYPE_3_BODY_LENGTH_OFFSET, (uint)aBodyEncrypted.Length);

            var aToSend = aHeader.Concat(aSymetricKeyEncrypted).Concat(aBodyEncrypted).ToArray();

            var aToSendHash = hash.Generate(new ByteArray(aToSend));
            logger.Debug($"Sending Secure Message {BitConverter.ToString(aToSendHash)}, Length:({aBodyEncrypted?.Length})");
            return this.textPlainTCPChannel.Send(aToSend, iOffset, aToSend.Length);
        }

        const short LITTLE_BUFFER_SIZE = 1024;
        const short MIDLE_BUFFER_SIZE = 1024 * 2;
        const short BIG_BUFFER_SIZE = 1024 * 4;

        public const byte HEADER_TYPE_1_SIZE = 4 + 4;
        public const byte HEADER_TYPE_2_SIZE = 4 + 4;
        public const byte HEADER_TYPE_3_SIZE = 4 + 4 + 4;

        public const byte HEADER_MAJOR_OFFSET = 1;
        public const byte HEADER_MINOR_OFFSET = 0;
        public const byte HEADER_FLAGS_OFFSET = 2;
        public const byte HEADER_TYPE_OFFSET = 3;

        public const byte HEADER_PUBKEY_LENGTH_OFFSET = 4;
        public const byte HEADER_PUBKEY_OFFSET = HEADER_PUBKEY_LENGTH_OFFSET + sizeof(int);
        public const byte HEADER_TYPE_3_SYMKEY_LENGTH_OFFSET = 4;
        public const byte HEADER_TYPE_3_BODY_LENGTH_OFFSET = 8;

        public const byte HEADER_TYPE_3_BODY_LENGTH_SIZE = 4;

        public const byte TYPE_FIRST_HANDSHAKE = 1;
        public const byte TYPE_PERSISTENT_HANDSHAKE = 2;
        public const byte TYPE_MESSAGE = 3;

        public const byte FLAGS_PPK_FOUND = 0x80;
        public const byte FLAGS_EXPIRED = 0x40;
        public const byte FLAGS_SERVER_REQUEST = 0x01;

        private readonly IAsymetricCrypt asymetricForDecrypt;
        private readonly IClientConnector textPlainTCPChannel;
        private readonly ISerializer serializeT;
        private readonly ISymetricCrypt symetric;

        private PublicKey remotePublicKey
        {
            set
            {
                this.asymetricToEncrypt.SetPublicKey(value);
            }
            get { return this.asymetricToEncrypt.GetPublicKey(); }
        }

        private PrivateKey thisPrivateKey
        {
            get
            {
                return this.asymetricForDecrypt.GetPrivateKey();
            }
            set
            {
                this.asymetricForDecrypt.SetPrivateKey(value);
            }
        }

        public string RemoteEndPoint
        {
            get
            {
                return this.textPlainTCPChannel?.RemoteEndPoint;
            }
        }

        public string LocalEndPoint
        {
            get
            {
                return this.textPlainTCPChannel?.LocalEndPoint;
            }
        }

        private readonly ILogger logger;
        private readonly IHash hash;
        private readonly IAsymetricCrypt asymetricToEncrypt;
        private readonly IPersistentPrivateKeyProvider IPersistentPrivateKeyProvider;
        private readonly ICompression compression;
        private SLJPOption sljpOptions;
        private readonly ISecureProtocolHandshake secureProtocolHandshake;
        private readonly IAsymetricCrypt firstAsymetricHandshakeToDecrypt;
        private readonly IAsymetricCrypt firstAsymetricHandshakeToEncrypt;
    }
}
