using System;
using System.Collections.Generic;
using BoilerplateBuilders.Reflection;

namespace BoilerplateBuilders.ToString.Format
{
    /// <summary>
    /// Converts set of members operations to function which returns string representation of an object.  
    /// </summary>
    public interface IToStringFormat
    {
        /// <summary>
        /// Converts set of members operations to function returning string representation of an object.
        /// </summary>
        /// <param name="operations">Set of formatting member operations.</param>
        /// <returns>
        /// Function converting object to string representation.
        /// </returns>
        Func<object, string> Build(IEnumerable<BuilderMemberOperation<Func<object, string>>> operations);
    }
}