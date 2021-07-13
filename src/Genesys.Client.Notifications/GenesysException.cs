using System;
using System.Collections.Generic;
using System.Text;

namespace Genesys.Client.Notifications
{
    public class GenesysException : Exception
    {
        /// <summary>
        /// Gets or sets the error code (HTTP status code)
        /// </summary>
        /// <value>The error code (HTTP status code).</value>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the error content (body json object)
        /// </summary>
        /// <value>The error content (HTTP response body).</value>
        public dynamic ErrorContent { get; private set; }

        /// <summary>
        /// Gets or sets the HTTP headers
        /// </summary>
        /// <value>HTTP headers</value>
        public IDictionary<string, string> Headers { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenesysException"/> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        public GenesysException(string message): base (message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenesysException"/> class.
        /// </summary>
        /// <param name="errorCode">HTTP status code.</param>
        /// <param name="message">Error message.</param>
        public GenesysException(int errorCode, string message) : base(message)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// </summary>
        /// <param name="errorCode">HTTP status code.</param>
        /// <param name="message">Error message.</param>
        /// <param name="errorContent">Error content.</param>
        /// <param name="headers">HTTP headers.</param>
        public GenesysException(int errorCode, string message, IDictionary<string, string> headers = null) : base(message)
        {
            this.ErrorCode = errorCode;
            this.Headers = headers;
        }
    }
}
