using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Common
{
    public class RingList<TE> : LinkedList<TE>
    {
		private readonly int _limit;

		public RingList(int limit)
		{
			_limit = limit;
		}

		public new void AddFirst(TE value)
		{
			if(Count > _limit)
				RemoveLast();
			base.AddFirst(new LinkedListNode<TE>(value));
		}

	}
}
