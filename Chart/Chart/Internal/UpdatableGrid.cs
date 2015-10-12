using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Controls;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public class UpdatableGrid : Grid, IUpdatable
    {
        public IUpdatable Parent
        {
            get
            {
                return (IUpdatable)null;
            }
        }

        public void Update()
        {
            EnumerableFunctions.ForEachWithIndex<IUpdatable>(Enumerable.OfType<IUpdatable>((IEnumerable)this.Children), (Action<IUpdatable, int>)((item, index) => item.Update()));
        }
    }
}
