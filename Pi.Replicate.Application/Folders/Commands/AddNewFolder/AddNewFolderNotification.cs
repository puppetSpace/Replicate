using MediatR;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Commands.AddNewFolder
{

	public interface IAddNewFolderObserver
	{
		void Notify(Folder newFolder);
	}

	public class AddNewFolderSubject
	{
		private List<IAddNewFolderObserver> _observers = new List<IAddNewFolderObserver>();
		public IDisposable Subscribe(IAddNewFolderObserver addNewFolderObserver)
		{
			return new Subscription(_observers, addNewFolderObserver);
		}

		public void NotifyChange(Folder newFolder)
		{
			foreach (var observer in _observers)
				observer.Notify(newFolder);
		}

		private class Subscription : IDisposable
		{
			private readonly List<IAddNewFolderObserver> _observers;
			private readonly IAddNewFolderObserver _observer;

			public Subscription(List<IAddNewFolderObserver> observers, IAddNewFolderObserver observer)
			{
				_observers = observers;
				_observer = observer;

				observers.Add(observer);
			}

			public void Dispose()
			{
				_observers.Remove(_observer);
			}
		}
	}
}
