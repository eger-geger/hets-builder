using System;
using System.Collections.Generic;
using BoilerplateBuilders.Reflection;

namespace BoilerplateBuilders.ToString.Format
{
    /// <summary>
    /// Combines set of <see cref="object.ToString"/> functions applied to object members into final function
    /// returning string representation of whole object.
    /// </summary>
    public interface IToStringFormat
    {
        /// <summary>
        /// Builds <see cref="object.ToString"/> function from individual <see cref="object.ToString"/> applied to
        /// its' members.
        /// </summary>
        /// <param name="operations">Sequence of member specific <see cref="object.ToString"/> functions.</param>
        /// <returns>Function returning string representation of an object.</returns>
        Func<object, string> Build(IEnumerable<MemberFunction<Func<object, string>>> operations);
    }
}