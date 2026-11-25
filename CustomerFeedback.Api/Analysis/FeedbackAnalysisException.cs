using Microsoft.AspNetCore.Http;

namespace CustomerFeedback.Api.Analysis;

public abstract class FeedbackAnalysisException : Exception
{
    protected FeedbackAnalysisException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    public abstract int StatusCode { get; }
}

public sealed class FeedbackAnalysisNotConfiguredException : FeedbackAnalysisException
{
    public FeedbackAnalysisNotConfiguredException()
        : base("Feedback analysis is not configured. Set Groq:ApiKey.")
    {
    }

    public override int StatusCode => StatusCodes.Status503ServiceUnavailable;
}

public sealed class FeedbackAnalysisTimeoutException : FeedbackAnalysisException
{
    public FeedbackAnalysisTimeoutException(Exception? innerException = null)
        : base("The AI provider did not respond in time.", innerException)
    {
    }

    public override int StatusCode => StatusCodes.Status504GatewayTimeout;
}

public sealed class FeedbackAnalysisProviderException : FeedbackAnalysisException
{
    public FeedbackAnalysisProviderException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    public override int StatusCode => StatusCodes.Status502BadGateway;
}

public sealed class FeedbackAnalysisInvalidResponseException : FeedbackAnalysisException
{
    public FeedbackAnalysisInvalidResponseException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    public override int StatusCode => StatusCodes.Status502BadGateway;
}