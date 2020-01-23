using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Trail365.Graphics.Internal

{
    #region Exception Types

    [Serializable]
    internal sealed class GuardAssertException : SystemException, ISerializable
    {
        private const int E_FAIL = -2147467259;
        private string m_paramName;

        ///// <summary>Initializes a new instance of the <see cref="T:System.ArgumentException"></see> class.</summary>
        //public GuardAssertException() : base(Environment.GetResourceString("Arg_ArgumentException"))
        //{
        //    base.SetErrorCode(-2147024809);
        //}

        /// <summary>Initializes a new instance of the <see cref="T:System.ArgumentException"></see> class with a specified error message.</summary>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        public GuardAssertException(string message)
            : base(message)
        {
            base.HResult = E_FAIL;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ArgumentException"></see> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception. </param>
        public GuardAssertException(string message, Exception innerException)
            : base(message, innerException)
        {
            base.HResult = E_FAIL;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ArgumentException"></see> class with a specified error message and the name of the parameter that causes this exception.</summary>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        /// <param name="paramName">The name of the parameter that caused the current exception. </param>
        public GuardAssertException(string message, string paramName)
            : base(message)
        {
            this.m_paramName = paramName;
            base.HResult = E_FAIL;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ArgumentException"></see> class with a specified error message, the parameter name, and a reference to the inner exception that is the cause of this exception.</summary>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        /// <param name="paramName">The name of the parameter that caused the current exception. </param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception. </param>
        public GuardAssertException(string message, string paramName, Exception innerException)
            : base(message, innerException)
        {
            this.m_paramName = paramName;
            base.HResult = E_FAIL;
        }

        /// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> object with the parameter name and additional exception information.</summary>
        /// <param name="context">The contextual information about the source or destination. </param>
        /// <param name="info">The object that holds the serialized object data. </param>
        /// <exception cref="T:System.ArgumentNullException">The info object is a null reference (Nothing in Visual Basic). </exception>
        /// <filterpriority>2</filterpriority>
        /// <PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" /></PermissionSet>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("ParamName", this.m_paramName, typeof(string));
        }

        /// <summary>Gets the error message and the parameter name, or only the error message if no parameter name is set.</summary>
        /// <returns>A text string describing the details of the exception. The value of this property takes one of two forms: Condition Value The paramName is a null reference (Nothing in Visual Basic) or of zero length. The message string passed to the constructor. The paramName is not null reference (Nothing in Visual Basic) and it has a length greater than zero. The message string appended with the name of the invalid parameter. </returns>
        /// <filterpriority>1</filterpriority>
        public override string Message
        {
            get
            {
                string message = base.Message;
                if (!string.IsNullOrEmpty(m_paramName))
                {
                    return (message + Environment.NewLine + string.Format(CultureInfo.CurrentCulture, "Parameter: " + m_paramName));
                }
                return message;
            }
        }

        /// <summary>Gets the name of the parameter that causes this exception.</summary>
        /// <returns>The parameter name.</returns>
        /// <filterpriority>1</filterpriority>
        public string ParamName
        {
            get
            {
                return this.m_paramName;
            }
        }
    }

    #endregion Exception Types
}
