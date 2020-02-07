using System;
using System.Collections.Generic;
using BoilerplateBuilders.Reflection;

namespace BoilerplateBuilders.ToString
{
    /// <summary>
    ///     Combines set of <see cref="object.ToString" /> functions applied to object members into final function
    ///     returning string representation of whole object.
    /// </summary>
    public interface IFormatterFactory
    {
        /// <summary>
        ///     Builds formatting function from sequence of formatting operations
        ///     applied to corresponding object's members.
        /// </summary>
        /// <param name="members">Sequence of formatting members and matching operations.</param>
        /// <returns>Function converting object to string representation.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="members" /> is null.
        /// </exception>
        Func<object, string> CreateToStringFunction(IEnumerable<MemberContext<Func<object, string>>> members);
    }
}