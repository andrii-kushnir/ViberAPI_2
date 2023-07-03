using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ViberAPI.Models
{
    public class WebhookResponse
    {
		[JsonProperty("event_types")]
		public ICollection<EventType> EventTypes { get; set; }
		[JsonProperty("status")]
		public ErrorCode Status { get; set; }
		[JsonProperty("status_message")]
		public string StatusMessage { get; set; }
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum EventType
	{
		/// <summary>
		/// Event 'delivered'.
		/// </summary>
		[EnumMember(Value = "delivered")]
		Delivered = 1,

		/// <summary>
		/// Event 'seen'.
		/// </summary>
		[EnumMember(Value = "seen")]
		Seen = 2,

		/// <summary>
		/// Event 'failed'.
		/// </summary>
		[EnumMember(Value = "failed")]
		Failed = 3,

		/// <summary>
		/// Event 'subscribed'.
		/// </summary>
		[EnumMember(Value = "subscribed")]
		Subscribed = 4,

		/// <summary>
		/// Event 'unsubscribed'.
		/// </summary>
		[EnumMember(Value = "unsubscribed")]
		Unsubscribed = 5,

		/// <summary>
		/// Event 'conversation_started'.
		/// </summary>
		[EnumMember(Value = "conversation_started")]
		ConversationStarted = 6,

		/// <summary>
		/// Event 'message'.
		/// </summary>
		[EnumMember(Value = "message")]
		Message = 7,

		/// <summary>
		/// Event 'webhook'.
		/// </summary>
		[EnumMember(Value = "webhook")]
		Webhook = 8,

		/// <summary>
		/// Event 'action'.
		/// </summary>
		[EnumMember(Value = "action")]
		Action = 9,

		/// <summary>
		/// Event 'client_status'.
		/// </summary>
		[EnumMember(Value = "client_status")]
		ClientStatus = 10
	}

	public enum ErrorCode
	{
		/// <summary>
		/// Success.
		/// </summary>
		Ok = 0,

		/// <summary>
		/// The webhook URL is not valid.
		/// </summary>
		InvalidUrl = 1,

		/// <summary>
		/// The authentication token is not valid.
		/// </summary>
		InvalidAuthToken = 2,

		/// <summary>
		/// There is an error in the request itself (missing comma, brackets, etc.).
		/// </summary>
		BadData = 3,

		/// <summary>
		/// Some mandatory data is missing.
		/// </summary>
		MissingData = 4,

		/// <summary>
		/// The receiver is not registered to Viber.
		/// </summary>
		ReceiverNotRegistered = 5,

		/// <summary>
		/// The receiver is not subscribed to the account.
		/// </summary>
		ReceiverNotSubscribed = 6,

		/// <summary>
		/// The account is blocked.
		/// </summary>
		PublicAccountBlocked = 7,

		/// <summary>
		/// The account associated with the token is not a account.
		/// </summary>
		PublicAccountNotFound = 8,

		/// <summary>
		/// The account is suspended.
		/// </summary>
		PublicAccountSuspended = 9,

		/// <summary>
		/// No webhook was set for the account.
		/// </summary>
		WebhookNotSet = 10,

		/// <summary>
		/// The receiver is using a device or a Viber version that don’t support accounts.
		/// </summary>
		ReceiverNoSuitableDevice = 11,

		/// <summary>
		/// Rate control breach.
		/// </summary>
		TooManyRequests = 12,

		/// <summary>
		/// Maximum supported account version by all user’s devices is less than the 'minApiVersion' in the message.
		/// </summary>
		ApiVersionNotSupported = 13,

		/// <summary>
		/// MinApiVersion is not compatible to the message fields.
		/// </summary>
		IncompatibleWithVersion = 14,

		/// <summary>
		/// The account is not authorized.
		/// </summary>
		PublicAccountNotAuthorized = 15,

		/// <summary>
		/// Inline message not allowed.
		/// </summary>
		InchatReplyMessageNotAllowed = 16,

		/// <summary>
		/// The account is not inline.
		/// </summary>
		PublicAccountIsNotInline = 17,

		/// <summary>
		/// Failed to post to public account. The bot is missing a Public Chat interface.
		/// </summary>
		NoPublicChat = 18,

		/// <summary>
		/// Cannot send broadcast message.
		/// </summary>
		CannotSendBroadcast = 19,

		/// <summary>
		/// General error.
		/// </summary>
		GeneralError = -1
	}
}
