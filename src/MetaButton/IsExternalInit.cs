using System.ComponentModel;

namespace System.Runtime.CompilerServices {
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit { }
}

namespace System.Runtime.CompilerServices {
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class CallerArgumentExpressionAttribute : Attribute {
        public string ParameterName {
            get; set;
        }
        public CallerArgumentExpressionAttribute(string parameterName) {
            ParameterName = parameterName;
        }
    }
}