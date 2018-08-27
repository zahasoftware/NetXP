using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.MVVM
{
    //Copy of IMessagingCenter
    //
    // Summary:
    //     Enables view models and other components to communicate by adhering to a message
    //     contract.
    //
    // Remarks:
    //     To be added.
    public interface IMessagingCenter
    {
        //
        // Summary:
        //     Sends a named message with the specified arguments.
        //
        // Parameters:
        //   sender:
        //     The instance that is sending the message. Typically, this is specified with the
        //     this keyword used within the sending object.
        //
        //   message:
        //     The message that will be sent to objects that are listening for the message from
        //     instances of type TSender.
        //
        //   args:
        //     The arguments that will be passed to the listener's callback.
        //
        // Type parameters:
        //   TSender:
        //     The type of object that sends the message.
        //
        //   TArgs:
        //     The type of the objects that are used as message arguments for the message.
        //
        // Remarks:
        //     To be added.
        void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class;
        //
        // Summary:
        //     Sends a named message that has no arguments.
        //
        // Parameters:
        //   sender:
        //     The instance that is sending the message. Typically, this is specified with the
        //     this keyword used within the sending object.
        //
        //   message:
        //     The message that will be sent to objects that are listening for the message from
        //     instances of type TSender.
        //
        // Type parameters:
        //   TSender:
        //     The type of object that sends the message.
        //
        // Remarks:
        //     To be added.
        void Send<TSender>(TSender sender, string message) where TSender : class;
        //
        // Summary:
        //     Run the callback on subscriber in response to parameterized messages that are
        //     named message and that are created by source.
        //
        // Parameters:
        //   subscriber:
        //     The object that is subscribing to the messages. Typically, this is specified
        //     with the this keyword used within the subscribing object.
        //
        //   message:
        //     The message that will be sent to objects that are listening for the message from
        //     instances of type TSender.
        //
        //   callback:
        //     A callback, which takes the sender and arguments as parameters, that is run when
        //     the message is received by the subscriber.
        //
        //   source:
        //     The object that will send the messages.
        //
        // Type parameters:
        //   TSender:
        //     The type of object that sends the message.
        //
        //   TArgs:
        //     The type of the objects that are used as message arguments for the message.
        //
        // Remarks:
        //     To be added.
        void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source = null) where TSender : class;
        //
        // Summary:
        //     Run the callback on subscriber in response to messages that are named message
        //     and that are created by source.
        //
        // Parameters:
        //   subscriber:
        //     The object that is subscribing to the messages. Typically, this is specified
        //     with the this keyword used within the subscribing object.
        //
        //   message:
        //     The message that will be sent to objects that are listening for the message from
        //     instances of type TSender.
        //
        //   callback:
        //     A callback, which takes the sender and arguments as parameters, that is run when
        //     the message is received by the subscriber.
        //
        //   source:
        //     The object that will send the messages.
        //
        // Type parameters:
        //   TSender:
        //     The type of object that sends the message.
        //
        // Remarks:
        //     To be added.
        void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source = null) where TSender : class;
        //
        // Summary:
        //     Unsubscribes from the specified parameterless subscriber messages.
        //
        // Parameters:
        //   subscriber:
        //     The object that is subscribing to the messages. Typically, this is specified
        //     with the this keyword used within the subscribing object.
        //
        //   message:
        //     The message that will be sent to objects that are listening for the message from
        //     instances of type TSender.
        //
        // Type parameters:
        //   TSender:
        //     The type of object that sends the message.
        //
        //   TArgs:
        //     The type of the objects that are used as message arguments for the message.
        //
        // Remarks:
        //     To be added.
        void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class;
        //
        // Summary:
        //     Unsubscribes a subscriber from the specified messages that come from the specified
        //     sender.
        //
        // Parameters:
        //   subscriber:
        //     The object that is subscribing to the messages. Typically, this is specified
        //     with the this keyword used within the subscribing object.
        //
        //   message:
        //     The message that will be sent to objects that are listening for the message from
        //     instances of type TSender.
        //
        // Type parameters:
        //   TSender:
        //     The type of object that sends the message.
        //
        // Remarks:
        //     To be added.
        void Unsubscribe<TSender>(object subscriber, string message) where TSender : class;
    }
}
