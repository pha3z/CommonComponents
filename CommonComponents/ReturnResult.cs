using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Common
{
	public static class ReturnResultObjectExtensions
    {
		public static async Task<R<T>> ToOkResult<T>(this Task<T> task) => R.Ok(await task);
		public static R<T> ToOkResult<T>(this T thing ) => R.Ok(thing);
		
		public static async Task ThrowOnError(this Task<R> t)
		{
			var r = await t;
			if (r.isErr)
				throw new Exception(r.JoinErrors());
		}

		public static async Task<T> GetValOrThrow<T>(this Task<R<T>> t, string additionalErrorMsg = null)
		{
			var r = await t;
			if (r.isErr)
				throw new Exception(additionalErrorMsg + " " + r.JoinErrors());

			return r.val;
		}
	}

	public class R
	{
		/// <summary>
		/// Some errors may return an ok/usable value even if an error was reported. In such a case, this will be true
		/// </summary>
		public bool isValueOk { get; protected set; } = true;

		/// <summary>Whether or not the result produced an error</summary>
		public bool isErr { get; protected set; }

		/// <summary>A textual error message. Will never be null</summary>
		public string err { get; protected set; }

		/// <summary>If an exception occurred, the exception may be retrieved here</summary>
		public Exception original_exception { get; protected set; }

		public bool hasMultipleErrors { get; protected set; }

		public string[] agg_errors { get; protected set; }

		/// <summary>
		/// Don't call this constructor directly.  use Result.Err&lt;T&gt; instead. Its much easier.
		/// </summary>
		/// <param name="Val">the result value. May be primitive, struct, or object</param>
		/// <param name="is_err">Whether or not the method caused an error</param>
		/// <param name="errMsg">The error message if any. Null will be converted to empty string</param>
		/// <param name="ex">The exception if one occurred</param>
		internal R(bool is_err, string errMsg = null, Exception ex = null, R prevErroredReturn = null)
		{
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
						agg_errors[i + 1] = prevErroredReturn.agg_errors[i];
				}
				else
					agg_errors = new string[1];

				//append the primary error
				agg_errors[0] = prevErroredReturn.err ?? "";
			}

			//There should only ever be one originating exception
			//We don't need to aggregate exceptions between returns. Just reference the original
			if (prevErroredReturn.original_exception != null)
				original_exception = prevErroredReturn.original_exception;
		}

		public void ThrowOnError()
        {
			if (isErr)
				throw new Exception(JoinErrors());
        }

		/// <summary>
		/// Joins primary error with aggregated errors.
		/// </summary>
		/// <param name="separator"></param>
		/// <param name="reverse"></param>
		/// <param name="includeExceptionMessage">If the error originated from an exception, the exception message can be included.</param>
		/// <returns></returns>
		public string JoinErrors(string separator = " ", bool reverse = false, bool includeExceptionMessage = true)
        {
			return String.Join(separator, 
				reverse 
				? AllErrors().Reverse() 
				: AllErrors());
        }

		/// <summary>
		/// Concatenates primary error with aggregated errors.
		/// </summary>
		/// <param name="includeExceptionMessage">If the error originated from an exception, the exception message can be included.</param>
		/// <returns></returns>
		public IEnumerable<string> AllErrors(bool includeExceptionMessage = true)
        {
			var aggregatedErrors = (agg_errors ?? new string[0]) as IEnumerable<string>;
			var errors = err.AsSet().Concat(aggregatedErrors);
			if (original_exception != null)
				return errors.Concat(original_exception.Message.AsSet());
			else
				return errors;
		}

		public static R OK => R.Ok();

		public static R Ok()
			=> new R(is_err: false);

		public static R ErrFromException(string err, Exception ex = null)
			=> new R(is_err: true, err, ex);

		public static R Err(string err, R lastErroredReturn = null)
			=> new R(is_err: true, err, ex: null, lastErroredReturn);

		public static R<T> Ok<T>(T val)
			=> new R<T>(valueIsOk: true, val, is_err: false);

		/// <summary>
		/// Creates a new Error Result. You may include the exception if an exception caused this error.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="err">The error message</param>
		/// <param name="ex">Optional: include the exception if one occurred</param>
		/// <returns></returns>
		public static R<T> ErrFromException<T>(string err, Exception ex = null)
			=> new R<T>(valueIsOk: false, default(T), is_err: true, err, ex);

		/// <summary>
		/// Creates a new Error Result. You may include the exception if an exception caused this error.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="err">The error message</param>
		/// <param name="ex">Optional: include the exception if one occurred</param>
		/// <returns></returns>
		public static R<T> Err<T>(string err, R lastErroredReturn = null)
			=> new R<T>(valueIsOk: false, default(T), is_err: true, err, ex: null, lastErroredReturn);

		/// <summary>
		/// Creates a new Error Result. You may include the exception if an exception caused this error.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="val">The result value which may be null</param>
		/// <param name="err">The error message</param>
		/// <param name="ex">Optional: include the exception if one occurred</param>
		/// <returns></returns>
		public static R<T> ErrFromException_ValueOk<T>(T val, string err, Exception ex = null)
			=> new R<T>(valueIsOk: true, val, is_err: true, err, ex);

		/// <summary>
		/// Creates a new Error Result. You may include the exception if an exception caused this error.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="val">The result value which may be null</param>
		/// <param name="err">The error message. Null will be converted to empty string.</param>
		/// <param name="lastErroredReturn">Optional: include return result of a previous method call if it too was an error. Errors will be aggregated and originating exception will be transferred to the new return result.</param>
		/// <returns></returns>
		public static R<T> Err_ValueOk<T>(T val, string err, R lastErroredReturn = null)
			=> new R<T>(valueIsOk: true, val, is_err: true, err, ex: null, lastErroredReturn);
	
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
		public T GetValOrThrow(string additionalErrorMsg = null)
		{
			if (this.isErr)
				throw new Exception(additionalErrorMsg + " " + JoinErrors());

			return this.val;
		}

		public R<K> Transform<K>(string newErrMsg = null)
		{
			return new R<K>(isValueOk, default(K), isErr, newErrMsg ?? "Deferred error.", original_exception, this);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="alternateValue"></param>
		/// <param name="substituteOnError_EvenWhenReturnedValueIsOk">if result is error but the returned value is marked ok, then it is use. Setting this flag to true will force substitution with alternateValue even if returned value is ok</param>
		/// <returns></returns>
		public T GetVal_or_SubstituteOnError(T alternateValue, bool substituteOnError_EvenWhenReturnedValueIsOk = false)
        {
			if (this.isErr)
			{
				//if values ok and we're not doing hard error substitution, then get hte value.
				if (isValueOk && !substituteOnError_EvenWhenReturnedValueIsOk)
					return this.val;
				else
					return alternateValue;
			}
			else
				return this.val;
        }
		/// <summary>
		/// Don't call this constructor directly.  use Result.Err&lt;T&gt; instead. Its much easier.
		/// </summary>
		/// <param name="Val">the result value. May be primitive, struct, or object</param>
		/// <param name="is_err">Whether or not the method caused an error</param>
		/// <param name="errMsg">The error message if any. Null will be converted to empty string</param>
		/// <param name="ex">The exception if one occurred</param>
		internal R(bool valueIsOk, T Val, bool is_err, string errMsg = null, Exception ex = null, R prevErroredReturn = null) : base(is_err, errMsg, ex, prevErroredReturn)
		{
			isValueOk = valueIsOk;
			val = Val;
		}


	}

}
