using System;
using System.Reactive;
using System.Windows.Controls;
using static System.Reactive.Linq.Observable;

namespace lens2.Ext
{
    /// <summary>
    /// Represents Extension methods for Button.
    /// </summary>
    static class ButtonExt
    {
        /// <summary>
        /// Represents a Stream of Click Event of a Button.
        /// </summary>
        /// <param name="button">The Source Button</param>
        /// <returns>Stream of </returns>
        internal static IObservable<EventPattern<object>> StreamClickEvent (this Button button) =>
            FromEventPattern(button, "Click");
    }
} 