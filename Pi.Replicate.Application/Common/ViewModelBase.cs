using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public void ResetState()
		{
			State = ViewModelState.Unchanged;
		}

		public void SetAsNew()
		{
			State = ViewModelState.New;
		}

		public bool IsChanged => State == ViewModelState.Changed; 

		public bool IsNew => State == ViewModelState.New;

		public ViewModelState State { get; private set; }

		protected void Set<TE>(ref TE oldValue, TE newValue, [CallerMemberName]string membername = "")
		{
			if (EqualityComparer<TE>.Default.Equals(oldValue, newValue))
				return;

			oldValue = newValue;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(membername));
			State = ViewModelState.Changed;
		}
	}

	public enum ViewModelState
	{
		Unchanged = 0,
		Changed = 1,
		New = 2
	}
}
