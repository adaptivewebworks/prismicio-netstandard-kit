namespace prismic
{
    public class Error: System.Exception
	{
		public enum ErrorCode {
			MALFORMED_URL,
			AUTHORIZATION_NEEDED, 
			INVALID_TOKEN,
			UNEXPECTED
		}

        public ErrorCode Code { get; }
        public Error(ErrorCode code, string message) : base(message) => this.Code = code;

        public override string ToString() => $"[{Code}] {base.Message}";

    }
}

