using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public static class DoOnce
    {
        private static HashSet<string> _performedActions = new HashSet<string>();

        public static void Do(string uniqueId, Action a)
        {
            if (_performedActions.Contains(uniqueId))
                return;

            _performedActions.Add(uniqueId);
            a();
        }
    }
}
