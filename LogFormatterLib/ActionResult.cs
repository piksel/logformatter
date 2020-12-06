using System;
using System.Collections.Generic;

namespace LogFormatter
{
    internal class ActionResult<TFlow>
    {
        private readonly TFlow _flow;

        public ActionResult(TFlow flow)
        {
            _flow = flow;
        }

        public TFlow And => _flow;
    }

    internal static class ActionResult
    {
        static readonly Dictionary<object, object> CachedResults = new Dictionary<object, object>();

        public static ActionResult<TFlow> For<TFlow>(TFlow flow)
        {
            if (flow is null) throw new ArgumentNullException(nameof(flow));

            if (!(CachedResults.TryGetValue(flow, out var cachedValue) &&
                  cachedValue is ActionResult<TFlow> actionResult))
            {
                actionResult = new ActionResult<TFlow>(flow);
                CachedResults[flow] = actionResult;
            }

            return actionResult;
        }
    }
}