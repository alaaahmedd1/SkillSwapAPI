using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Common.Errors;

public static class ApplicationErrors
{
    public static class Token
    {
        public static Error ExpiredAccessTokenInvalid =>
            Error.Unauthorized("Token.ExpiredAccessTokenInvalid", "Expired access token is not valid.");

        public static Error UserIdClaimInvalid =>
            Error.Unauthorized("Token.UserIdClaimInvalid", "Token does not contain a valid user identifier.");

        public static Error RefreshTokenExpired =>
            Error.Unauthorized("Token.RefreshTokenExpired", "Refresh token has expired or does not exist.");

        public static Error RefreshTokenReused =>
            Error.Unauthorized("Token.RefreshTokenReused", "Refresh token reuse was detected. Please sign in again.");

        public static Error GenerationFailed =>
            Error.Unexpected("Token.GenerationFailed", "An error occurred while generating the token.");
    }

    public static class Auth
    {
        public static Error InvalidCredentials =>
            Error.Unauthorized("Auth.InvalidCredentials", "Email or password is incorrect.");

        public static Error AccountLockedOut =>
            Error.Forbidden("Auth.AccountLockedOut", "Account is locked. Please try again later.");

        public static Error EmailAlreadyExists =>
            Error.Conflict("Auth.EmailAlreadyExists", "An account with this email already exists.");

        public static Error EmailNotVerified =>
            Error.Forbidden("Auth.EmailNotVerified", "Email address has not been verified. Please verify your email before logging in.");

        public static Error UserNotFound =>
            Error.NotFound("Auth.UserNotFound", "User not found.");

        public static Error NotFound(string identityId) =>
            Error.NotFound("User.NotFound", $"User with identity id '{identityId}' was not found.");
    }

    public static class Otp
    {
        public static Error Invalid =>
           Error.Validation("Otp.Invalid", "The OTP code is invalid or has expired.");

        public static Error SendFailed =>
            Error.Unexpected("Otp.SendFailed", "Failed to send OTP email. Please try again.");

        public static Error AlreadyVerified =>
            Error.Conflict("Otp.AlreadyVerified", "This email is already verified.");

        public static Error TooManyRequests =>
            Error.Failure("Otp.TooManyRequests", "Too many OTP requests. Please wait before trying again.");
    }

    public static class SocialAuth
    {
        public static Error InvalidToken =>
            Error.Unauthorized(
                "SocialAuth.InvalidToken",
                "The provided social token is invalid or has expired.");

        public static Error ProviderNotSupported =>
            Error.Validation(
                "SocialAuth.ProviderNotSupported",
                "The specified social provider is not supported.");

        public static Error EmailNotProvided =>
            Error.Validation(
                "SocialAuth.EmailNotProvided",
                "Could not retrieve email from the social provider. Please ensure email access is granted.");

        public static Error TokenVerificationFailed =>
            Error.Unexpected(
                "SocialAuth.TokenVerificationFailed",
                "An error occurred while verifying the social token.");
    }

    public static class Skills
    {
        public static readonly Error DuplicateSkillType =
            Error.Validation("Skills.DuplicateType", "This skill is already listed as the opposite type (Offered/Seeking) for this user.");
        public static readonly Error SkillNotFound =
            Error.Validation("Skills.NotFound", "The specified skill does not exist.");
        public static readonly Error UserSkillNotFound =
            Error.Validation("Skills.UserSkillNotFound", "The specified user skill entry was not found.");
        public static readonly Error UserSkillNotOwned =
            Error.Validation("Skills.NotOwned", "You do not have permission to modify this skill entry.");
    }

    public static class SwapRequests
    {
        public static readonly Error NotFound =
            Error.NotFound("SwapRequests.NotFound", "The swap request was not found.");
        public static readonly Error InvalidStatusTransition =
            Error.Conflict("SwapRequests.InvalidStatusTransition", "The swap request status does not allow this action.");
        public static readonly Error OnlyReceiverCanRespond =
            Error.Forbidden("SwapRequests.OnlyReceiverCanRespond", "Only the receiver can accept or reject this swap request.");
        public static readonly Error OnlyRequesterCanCancel =
            Error.Forbidden("SwapRequests.OnlyRequesterCanCancel", "Only the requester can cancel a pending swap request. The receiver must reject it instead.");
        public static readonly Error NotParticipant =
            Error.Forbidden("SwapRequests.NotParticipant", "You are not a participant in this swap request.");
        public static readonly Error ConcurrencyConflict =
            Error.Conflict("SwapRequests.ConcurrencyConflict", "The swap request was modified by another request. Please retry.");
    }

    public static class Chat
    {
        public static readonly Error ConversationNotFound =
            Error.NotFound("Chat.ConversationNotFound", "The conversation was not found.");
        public static readonly Error NotSwapParticipant =
            Error.Forbidden("Chat.NotSwapParticipant", "You are not a participant in this conversation.");
        public static readonly Error SwapNotActive =
            Error.Forbidden("Chat.SwapNotActive", "Messaging is only available for accepted or completed swaps.");
    }

    public static class Reviews
    {
        public static readonly Error SwapNotCompleted =
            Error.Validation("Reviews.SwapNotCompleted", "Reviews can only be submitted for completed swaps.");
        public static readonly Error NotParticipant =
            Error.Forbidden("Reviews.NotParticipant", "You are not a participant in this swap request.");
        public static readonly Error InvalidReviewee =
            Error.Validation("Reviews.InvalidReviewee", "The reviewee must be the other participant of the swap request.");
    }
}
