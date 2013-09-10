﻿using System.Collections.Generic;
using System.Net;

namespace TweetinCore.Events
{
    /// <summary>
    /// Delegate used to handle WebExceptions
    /// </summary>
    /// <param name="ex">WebException to handle</param>
    public delegate void WebExceptionHandlingDelegate(WebException ex);
 
    /// <summary>
    /// Delegate used to handle information retrieved and passed through a Dictionary[string, object]
    /// </summary>
    /// <param name="responseObject">Information retrieved</param>
    public delegate void ObjectResponseDelegate(Dictionary<string, object> responseObject);
    
    /// <summary>
    /// Delegate used to retrieve the verifier key given by Twitter
    /// when an application request a user token
    /// </summary>
    /// <param name="url">Validation url where the verifier key can be retrieved</param>
    /// <returns>Verifier key created by Twitter</returns>
    public delegate int RetrieveCaptchaDelegate(string url);

    /// <summary>
    /// Delegate used to handle queries that require the use of cursors
    /// </summary>
    /// <param name="responseObject">Response given by the Twitter API</param>
    /// <param name="previous_cursor">Cursor used during the previous query</param>
    /// <param name="next_cursor">Cursor to be used in the next query</param>
    public delegate void DynamicResponseDelegate(dynamic responseObject, long previous_cursor, long next_cursor);
}
