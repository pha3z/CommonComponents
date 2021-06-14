using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Common
{
	public class R
	{
		/// <summary>Whether or not the result produced an error</summary>
		public bool isErr { get; protected set; }

		/// <summary>A textual error message. Will never be null</summary>
		public string err { get; protected set; }

		/// <summary>If an exception occurred, the exception may be retrieved here</summary>
		public Exception original_exception { get; protected set; }

		public bool hasMultipleErrors { get; protected set; }

		public string[] agg_errors { get; protected set; }

		/// <summary>
		/// Joins primary error with aggregated errors.
		/// </summary>
		/// <param name="separator"></param>
		/// <param name="reverse"></param>
		/// <returns></returns>
		public string JoinErrors(string separator, bool reverse = false)
        {
			return String.Join(separator, 
				reverse 
				? AllErrors().Reverse() 
				: AllErrors());
        }

		/// <summary>
		/// Concatenates primary error with aggregated errors.
		/// </summary>
		/// <param name="reverse"></param>
		/// <returns></returns>
		public IEnumerable<string> AllErrors()
        {
			var aggregatedErrors = (agg_errors ?? new string[0]) as IEnumerable<string>;
			return err.AsSet().Concat(aggregatedErrors);
		}

		public static R<T> Ok<T>(T val)
			=> new R<T>(val, is_err: false);

		/// <summary>
		/// Creates a new Error Result. You may include the exception if an exception caused this error.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="err">The error message</param>
		/// <param name="ex">Optional: include the exception if one occurred</param>
		/// <returns></returns>
		public static R<T> ErrFromException<T>(string err, Exception ex = null)
			=> new R<T>(default(T), is_err: true, err, ex);


		/// <summary>
		/// Creates a new Error Result. You may include the exception if an exception caused this error.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="err">The error message</param>
		/// <param name="ex">Optional: include the exception if one occurred</param>
		/// <returns></returns>
		public static R<T> Err<T>(string err, R lastErroredReturn = null)
			=> new R<T>(default(T), is_err: true, err, ex: null, lastErroredReturn);

		/// <summary>
		/// Creates a new Error Result. You may include the exception if an exception caused this error.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="val">The result value which may be null</param>
		/// <param name="err">The error message</param>
		/// <param name="ex">Optional: include the exception if one occurred</param>
		/// <returns></returns>
		public static R<T> ErrFromException<T>(T val, string err, Exception ex = null)
			=> new R<T>(val, is_err: true, err, ex);

		/// <summary>
		/// Creates a new Error Result. You may include the exception if an exception caused this error.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="val">The result value which may be null</param>
		/// <param name="err">The error message. Null will be converted to empty string.</param>
		/// <param name="lastErroredReturn">Optional: include return result of a previous method call if it too was an error. Errors will be aggregated and originating exception will be transferred to the new return result.</param>
		/// <returns></returns>
		public static R<T> Err<T>(T val, string err, R lastErroredReturn = null)
			=> new R<T>(val, is_err: true, err, ex: null, lastErroredReturn);

	}

	public class R<T> : R
	{
		/// <summary>
		/// You should not depend on the result value if isErr == true. Value will depend on behavior defined by invoked method.
		/// </summary>
		public T val { get; private set; }

		/// <summary>
		/// Returns val or throws an exception if property isErr == true
		/// </summary>
		/// <param name="additionalErrorMsg">Optional: additional string to prefix to internal error message when creating exception.</param>
		/// <returns></returns>
		public T GetOrThrow(string additionalErrorMsg = null)
		{
			if (this.isErr)
				throw new Exception(additionalErrorMsg + " " + this.err);

			return this.val;
		}

		/// <summary>
		/// Don't call this constructor directly.  use Result.Err&lt;T&gt; instead. Its much easier.
		/// </summary>
		/// <param name="Val">the result value. May be primitive, struct, or object</param>
		/// <param name="is_err">Whether or not the method caused an error</param>
		/// <param name="errMsg">The error message if any. Null will be converted to empty string</param>
		/// <param name="ex">The exception if one occurred</param>
		internal R(T Val, bool is_err, string errMsg = null, Exception ex = null, R prevErroredReturn = null)
		{
			val = Val;
			isErr = is_err;
			err = errMsg ?? "";
			original_exception = ex;

			if (prevErroredReturn == null)
				return;

			//If we had a previous error
			//Collect its aggregated errors if any, and append its primary error
			if (prevErroredReturn.isErr)
            {
				hasMultipleErrors = true;

				//copy the aggregated errors
				if (prevErroredReturn.hasMultipleErrors)
				{
					agg_errors = new string[prevErroredReturn.agg_errors.Count() + 1];

					//Skip the first allocated error position
					//We'll write the newest error into that position.
					for (int i = 0; i < prevErroredReturn.agg_errors.Count(); i++)
						agg_errors[i+1] = prevErroredReturn.agg_errors[i];
				}
				else
					agg_errors = new string[1];

				//append the primary error
				agg_errors[0] = prevErroredReturn.err ?? "";
			}

			//There should only ever be one originating exception
			//We don't need to aggregate exceptions between returns. Just reference the original
			if(prevErroredReturn.original_exception != null)
				original_exception = prevErroredReturn.original_exception;
		}
	}

}
