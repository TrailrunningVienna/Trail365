using System;
using System.Diagnostics;
using System.Text;

namespace Trail365.Graphics.Internal
{
    /// <summary>
    /// Guard, the primary Exception Management Helper Class
    /// </summary>
    [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
    public static class Guard //WM 06/2017 public because used in other Assemblies that are part of the SAME Solution!
    {
        public static void ExpectException<E>(Action callback) where E : System.Exception
        {
            ExpectException<E>(callback, null);
        }

        public static void ExpectException<E>(Action callback, Func<E, bool> isExpected) where E : System.Exception
        {
            try
            {
                callback();
            }
            catch (System.Exception ex)
            {
                if (typeof(E).IsAssignableFrom(ex.GetType()))
                {
                    if (isExpected == null) return;
                    E casted = ex as E;
                    Guard.ArgumentNotNull(casted, "Internal Error");
                    if (isExpected(casted)) return;
                }
                throw new InvalidOperationException(string.Format("A exception from Type {0} was expected, a invalid exception from Type {1} was thrown!", typeof(E).FullName, ex.GetType().FullName));
            }
            throw new InvalidOperationException(string.Format("A exception from Type {0} was expected, NO exception was thrown!", typeof(E).FullName));
        }

        #region private members

        private const string DefaultAssertMessage = "Assertion failed!";
        private static readonly BooleanSwitch IgnoreAssertSwitch = new BooleanSwitch("IgnoreAssert", "Ignore Guard.Assert");

        #endregion private members

        /// <summary>
        /// ReadOnly Property, that retursn the current Value from  .Config File!
        /// if the switch is not defined in the App.Config, the default value is false!
        /// </summary>
        /// <value><c>true</c> if true, Asserts are ignored otherwise a invalid Assert causes a Exception.</value>
        public static bool IgnoreAssert
        {
            get
            {
                return IgnoreAssertSwitch.Enabled;
            }
        }

        /// <summary>
        /// a valid DataRow must be
        /// - not null
        /// - not deleted
        /// - not detached
        /// </summary>
        /// <param name="row">The row.</param>
        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void AssertValidDataRow(System.Data.DataRow row)
        {
            if (IgnoreAssert) return;
            Guard.AssertNotNull(row, "row");
            if (row.RowState == System.Data.DataRowState.Detached)
            {
                AssertionFail("RowState is 'Detached'", "row");
            }
            if (row.RowState == System.Data.DataRowState.Deleted)
            {
                AssertionFail("RowState is 'Deleted'", "row");
            }
        }

        //[Conditional("ASSERT"), System.Diagnostics.DebuggerNonUserCodeAttribute(), System.Diagnostics.DebuggerStepThrough()]
        //public static void AssertValidIdentfier(string name)
        //{
        //    if (IgnoreAssert) return;
        //    if (Irondev.Framework.Shared.Identifier.IsValid(name)) return;
        //    AssertionFail("Identifier is not CLS-Copiliant", name);
        //}

        //[System.Diagnostics.DebuggerNonUserCodeAttribute(), System.Diagnostics.DebuggerStepThrough()]
        //public static void ArgumentValidIdentifier(string identifier, string argumentName)
        //{
        //    Guard.ArgumentNotNullOrEmptyString(identifier, "identifier");
        //    if (Irondev.Framework.Shared.Identifier.IsValid(identifier) == false)
        //    {
        //        throw new ArgumentException("The identifier is not CLS-Compiliant: " + identifier, "identifier");
        //    }
        //}

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        private static void AssertionFail(string message, string argument)
        {
            StackTrace trace1 = new StackTrace(true);
            GuardAssertException ex = null;
            if (string.IsNullOrEmpty(argument))
            {
                ex = new GuardAssertException(message);
            }
            else
            {
                ex = new GuardAssertException(message, argument);
            }
            //TODO rausfinden wie tief im STack wir wirklich sind und StackTiefe(derzeit) 2 explizit setzen und berechnen
            //ist aber derzeit nicht notwendig solange AssertionFail nicht verschachtelt aufgerufen werden kann
            ex.Source = trace1.GetFrame(2).ToString();
            throw ex;
        }

        /// <summary>
        /// Checks for a condition and displays a message if the condition is false.
        /// </summary>
        /// <param name="condition">true to prevent a message being displayed; otherwise, false.</param>
        /// <param name="message">A message to display.</param>
        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void Assert(bool condition, string message)
        {
            if (IgnoreAssert) return;
            if (condition == true) return;
            AssertionFail(message, string.Empty);
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void AssertNotNullOrEmptyString(string argumentValue)
        {
            if (IgnoreAssert) return;
            if (string.IsNullOrEmpty(argumentValue))
            {
                AssertionFail("String is null or Empty", argumentValue);
            }
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void AssertNotNullOrEmptyString(string argumentValue, string message)
        {
            if (IgnoreAssert) return;
            ArgumentNotNullOrEmptyString(argumentValue, "argumentValue");
            ArgumentNotNull(message, "message");
            if (string.IsNullOrEmpty(argumentValue))
            {
                AssertionFail(message, argumentValue);
            }
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void AssertEnumValueIsDefined<T>(object value, string argumentName)
        {
            Guard.ArgumentNotNull(value, argumentName);
            if (Enum.IsDefined(typeof(T), value) == false)
                AssertionFail("Invalid Enum-Value for " + typeof(T).Name, argumentName);
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void AssertEnumValueIsDefined(Type enumType, object value, string argumentName)
        {
            Guard.ArgumentNotNull(enumType, "enumType");
            Guard.ArgumentNotNull(value, argumentName);
            if (Enum.IsDefined(enumType, value) == false)
                AssertionFail("Invalid Enum-Value", argumentName);
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void AssertNotNullOrEmptyList(System.Collections.IList list, string argumentName)
        {
            if (IgnoreAssert) return;
            if (list == null) AssertionFail("AssertNotNullOrEmptyList: Argument is null", argumentName);
            if (list.Count == 0)
            {
                AssertionFail("AssertNotNullOrEmptyList: list is empty", argumentName);
            }
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void AssertNotNull(object obj)
        {
            if (IgnoreAssert) return;
            if (obj != null) return;
            AssertionFail("Assertion (Not Null) failed!", string.Empty);
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void AssertNull(object obj)
        {
            if (IgnoreAssert) return;
            if (obj == null) return;
            AssertionFail("Assertion (MUST be Null) failed!", string.Empty);
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void AssertNull(object obj, string argument, string message)
        {
            if (IgnoreAssert) return;
            if (obj == null) return;
            AssertionFail(message, argument);
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void AssertFileExists(System.IO.FileInfo file)
        {
            if (IgnoreAssert) return;
            Guard.ArgumentNotNull(file, "file");
            file.Refresh(); //um falsch-Ergebnisse zu verhindern, Zeitintensiv aber egal im Assert!
            if (file.Exists == false)
            {
                AssertionFail("File not exists", file.FullName);
            }
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void AssertNotNull(object obj, string message)
        {
            if (IgnoreAssert) return;
            if (obj != null) return;
            AssertionFail("AssertNotNull: " + message, string.Empty);
        }

        /// <summary>
        /// Damit kann ein ganzer Delegate (anonymle Methode) injiziert werden zum Berechnen der Assertions!
        /// Wenn Asserts OFF sind wird die ganze Methode nicht compiliert!
        /// </summary>
        /// <param name="condition">The condition.</param>
        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void Assert(Func<bool> condition)
        {
            if (IgnoreAssert) return;
            if (condition() == true) return;
            AssertionFail(DefaultAssertMessage, string.Empty);
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void Assert(Func<bool> condition, string message)
        {
            if (IgnoreAssert) return;
            if (condition() == true) return;
            AssertionFail(message, string.Empty);
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void Assert(bool condition)
        {
            if (IgnoreAssert) return;
            if (condition == true) return;
            AssertionFail(DefaultAssertMessage, string.Empty);
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void Assert(bool condition, string message, string argument)
        {
            if (IgnoreAssert) return;
            if (condition == true) return;
            AssertionFail(message, argument);
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void ArgumentEnumValueIsDefined<T>(object value, string argumentName)
        {
            Guard.ArgumentNotNull(value, argumentName);
            Guard.ArgumentNotNullOrEmptyString(argumentName, "argumentName");
            if (Enum.IsDefined(typeof(T), value) == false)
                throw new ArgumentOutOfRangeException(argumentName, "Invalid enumeration value: " + typeof(T).Name);
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void ArgumentEnumValueIsDefined(Type enumType, object value, string argumentName)
        {
            Guard.ArgumentNotNull(enumType, "enumType");
            Guard.ArgumentNotNull(value, argumentName);
            Guard.ArgumentNotNullOrEmptyString(argumentName, "argumentName");
            if (Enum.IsDefined(enumType, value) == false)
                throw new ArgumentOutOfRangeException(argumentName, "Invalid enumeration value: " + enumType.Name);
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void ArgumentEnumValueIsDefined(Type enumType, object value, string argumentName, string message)
        {
            Guard.ArgumentNotNull(enumType, "enumType");
            Guard.ArgumentNotNull(value, argumentName);
            Guard.ArgumentNotNullOrEmptyString(argumentName, "argumentName");
            Guard.ArgumentNotNullOrEmptyString(message, "message");
            if (Enum.IsDefined(enumType, value) == false)
                throw new ArgumentOutOfRangeException(argumentName, message);
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void ArgumentValidDataRow(System.Data.DataRow dataRow, string argumentName)
        {
            Guard.ArgumentNotNull(dataRow, "dataRow");
            Guard.ArgumentNotNullOrEmptyString(argumentName, "argumentName");

            if (dataRow.RowState == System.Data.DataRowState.Detached)
            {
                Argument(false, argumentName, "DataRow is Detached (DataRowState)");
            }
            if (dataRow.RowState == System.Data.DataRowState.Deleted)
            {
                Argument(false, argumentName, "DataRow is Deleted (DataRowState)");
            }
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void ArgumentNotNullOrEmptyList(System.Collections.IList list, string argumentName)
        {
            if (list == null)
            {
                throw new ArgumentNullException(argumentName);
            }

            if (list.Count == 0)
            {
                throw new ArgumentException("List is empty", argumentName);
            }
        }

        /// <summary>
        /// Checks a string argument to ensure it isn't null or empty
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
#if DEBUG
#else

        [System.Diagnostics.DebuggerNonUserCodeAttribute(), System.Diagnostics.DebuggerStepThrough()]
#endif
        public static void ArgumentNotNullOrEmptyString(string argumentValue, string argumentName, string message)
        {
            if (string.IsNullOrEmpty(argumentValue))
            {
                throw new ArgumentNullException(message, new ArgumentNullException(message, argumentName));
            }
        }

        /// <summary>
        /// Checks a string argument to ensure it isn't null or empty
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
#if DEBUG
#else

        [System.Diagnostics.DebuggerNonUserCodeAttribute(), System.Diagnostics.DebuggerStepThrough()]
#endif
        public static void ArgumentNotNullOrEmptyString(string argumentValue, string argumentName)
        {
            if (string.IsNullOrEmpty(argumentValue))
            {
                string message = string.Format("Argument is null or empty string: {0}", argumentName);
                throw new ArgumentNullException(message, new ArgumentNullException(message, argumentName));
            }
        }

#if DEBUG
#else

        [System.Diagnostics.DebuggerNonUserCodeAttribute(), System.Diagnostics.DebuggerStepThrough()]
#endif
        public static void ArgumentNotNullOrEmptyStream(System.IO.Stream argumentStream, string argumentName)
        {
            if (argumentStream == null)
            {
                throw new ArgumentNullException(argumentName);
            }
            if (argumentStream.Length < 1)
            {
                string message = string.Format("Argument is empty stream: {0}", argumentName);
                throw new ArgumentNullException(message, new ArgumentNullException(message, argumentName));
            }
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void DirectoryNotFound(System.IO.DirectoryInfo directory)
        {
            DirectoryNotFound(directory, string.Empty);
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static string RemoveGuardCallFromStackTrace(string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace)) return string.Empty;
            string[] str = stackTrace.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (str.Length > 0)
            {
                if (str[0].Contains(typeof(Guard).FullName)) str[0] = string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            foreach (string s1 in str)
            {
                if (string.IsNullOrEmpty(s1) == false)
                {
                    sb.AppendLine(s1);
                }
            }
            return sb.ToString();
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void DirectoryNotFound(System.IO.DirectoryInfo directory, string message)
        {
            Guard.ArgumentNotNull(directory, "directory");

            if (directory.Exists) return;

            if (string.IsNullOrEmpty(message))
            {
                message = "Directory not found: {0}";
            }
            string s = string.Format(message, directory.FullName);
            throw new System.IO.DirectoryNotFoundException(s);
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void Application(string Message)
        {
            throw new ApplicationException(Message);
        }

        //DerName ist etwas lang, "object" wird entfernt
        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void AssertNotDisposed(bool isDisposed, string message)
        {
            if (!isDisposed) return;
            ObjectDisposed(isDisposed, string.Empty, message);
        }

        [Conditional("ASSERT"), DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void AssertNotDisposed(bool isDisposed)
        {
            if (!isDisposed) return;
            ObjectDisposed(isDisposed);
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void ObjectDisposed(bool isDisposed, string objectName)
        {
            ObjectDisposed(isDisposed, objectName, string.Empty);
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void ObjectDisposed(bool isDisposed, string objectName, string message)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(objectName, message);
            }
        }

        //Not Tested/Implemented
        /// <summary>
        /// geht davo aus, dass die Methode von INNERHALB des zu prüfendenObjektes ausgeführt wird,
        /// die Objektinstanz, Methode und Typ wird mittels StackTrace ermittel!
        /// </summary>
        /// <param name="isDisposed">if set to <c>true</c> [is disposed].</param>
        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void ObjectDisposed(bool isDisposed)
        {
            if (isDisposed)
            {
                string objectName = "unknown";
                string message = string.Empty;
                string source = string.Empty;
                StackTrace sttr = new StackTrace(1);
                if (sttr.FrameCount > 0) //defensiv
                {
                    StackFrame fr = sttr.GetFrame(0);
                    if (fr != null)
                    {
                        System.Reflection.MethodBase mb = fr.GetMethod();
                        if (mb != null)
                        {
                            objectName = mb.DeclaringType.FullName;
                            message = "MethodeName=" + mb.Name;
                            source = mb.DeclaringType.Assembly.GetName().Name;
                        }
                    }
                }

                ObjectDisposedException ode = new ObjectDisposedException(objectName, message);
                if (string.IsNullOrEmpty(source) == false)
                {
                    ode.Source = source;
                }
                throw ode;
            }
        }

        [DebuggerNonUserCode()]
        public static void ApplicationAssert(bool condition, string message)
        {
            if (condition) return;
            throw new ApplicationException(message);
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void ArgumentOutOfRange(System.Collections.IList list, int index, string argumentName)
        {
            ArgumentNotNullOrEmptyList(list, argumentName);
            if ((index < 0) || (index > list.Count - 1)) throw new ArgumentOutOfRangeException(argumentName, "Index out of range: " + index.ToString());
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void ItemNotFound(string ItemName)
        {
            throw new System.ArgumentOutOfRangeException(ItemName);
        }

#if DEBUG
#else

        [System.Diagnostics.DebuggerNonUserCodeAttribute(), System.Diagnostics.DebuggerStepThrough()]
#endif
        public static void Argument(bool isValid, string parameterName)
        {
            if (isValid == true) return;
            string msg = string.Format("Invalid Argument: {0}", parameterName);
            throw new ArgumentException(msg, new ArgumentException(msg, parameterName));
        }

        public static void StateIsValid(bool isValidState, string stateName)
        {
            if (isValidState == true) return;
            string msg = string.Format("Invalid state: {0}", stateName);
            throw new InvalidOperationException(msg);
        }

        public static void StateIsValid(bool isValidState, string stateName, string message)
        {
            if (isValidState == true) return;
            string msg = string.Format("Invalid state: {0}{1}{2}", stateName, Environment.NewLine, message);
            throw new InvalidOperationException(msg);
        }

        public static void StateIsTrue(bool argument, string stateName)
        {
            if (argument == true) return;
            string msg = string.Format("Invalid state (state is not True): {0}", stateName);
            throw new InvalidOperationException(msg);
        }

        public static void StateIsFalse(bool argument, string stateName)
        {
            if (argument == false) return;
            string msg = string.Format("Invalid state (state is not False): {0}", stateName);
            throw new InvalidOperationException(msg);
        }

#if DEBUG
# else

        [System.Diagnostics.DebuggerNonUserCodeAttribute(), System.Diagnostics.DebuggerStepThrough()]
#endif
        public static void ArgumentIsTrue(bool argument, string parameterName)
        {
            if (argument == true) return;
            string msg = string.Format("Invalid Argument (Argument is not True): {0}", parameterName);
            throw new ArgumentException(msg, new ArgumentException(msg, parameterName));
        }

#if DEBUG
# else

        [System.Diagnostics.DebuggerNonUserCodeAttribute(), System.Diagnostics.DebuggerStepThrough()]
#endif
        public static void ArgumentIsFalse(bool argument, string parameterName)
        {
            if (argument == false) return;
            string msg = string.Format("Invalid Argument (Argument is not False): {0}", parameterName);
            throw new ArgumentException(msg, new ArgumentException(msg, parameterName));
        }

        /// <summary>
        /// Throws a exception if the he specified Argument is not valid.
        /// </summary>
        /// <param name="isValid">if set to <c>true</c> [is valid].</param>
        /// <param name="parameterName">Value of the parameter.</param>
        /// <param name="message">The message.</param>
#if DEBUG
#else

        [System.Diagnostics.DebuggerNonUserCodeAttribute(), System.Diagnostics.DebuggerStepThrough()]
#endif
        public static void Argument(bool isValid, string parameterName, string message)
        {
            if (isValid == true) return;
            throw new ArgumentException(message, new ArgumentException(message, parameterName));
        }

        /// <summary>
        /// Checks an argument to ensure it isn't null
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void ArgumentNotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(argumentName);
        }

        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void InvalidOperation(string Message)
        {
            throw new InvalidOperationException(Message);
        }

        /// <summary>
        /// Checks an argument to ensure it isn't null
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void ArgumentNotNull(object argumentValue, string argumentName, string message)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(message, new ArgumentException(message, argumentName));
        }

        /// <summary>
        /// Checks an Enum argument to ensure that its value is defined by the specified Enum type.
        /// </summary>
        /// <param name="enumType">The Enum type the value should correspond to.</param>
        /// <param name="value">The value to check for.</param>
        /// <param name="argumentName">The name of the argument holding the value.</param>
        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void EnumValueIsDefined(Type enumType, object value, string argumentName)
        {
            Guard.ArgumentNotNull(enumType, "enumType");
            Guard.ArgumentNotNull(value, argumentName);
            if (Enum.IsDefined(enumType, value) == false)
                throw new ArgumentException("Invalid Enum-Value", argumentName);
        }

        /// <summary>
        /// Verifies that an argument type is assignable from the provided type (meaning
        /// interfaces are implemented, or classes exist in the base class hierarchy).
        /// </summary>
        /// <param name="assignee">The argument type.</param>
        /// <param name="providedType">The type it must be assignable from.</param>
        /// <param name="argumentName">The argument name.</param>
        [DebuggerNonUserCode(), System.Diagnostics.DebuggerStepThrough()]
        public static void TypeIsAssignableFromType(Type assignee, Type providedType, string argumentName)
        {
            Guard.ArgumentNotNull(providedType, "providedType");
            if (!providedType.IsAssignableFrom(assignee))
                throw new ArgumentException("Type not compatible for assign", argumentName);
        }
    }
}
