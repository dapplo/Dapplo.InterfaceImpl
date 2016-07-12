//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.InterfaceImpl
// 
//  Dapplo.InterfaceImpl is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.InterfaceImpl is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.InterfaceImpl. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System.ComponentModel;
using Dapplo.InterfaceImpl.Implementation;

#endregion

namespace Dapplo.InterfaceImpl.Extensions.Implementation
{
	/// <summary>
	///     This class implements the NotifyPropertyChanged extension logic,
	///     which automatically generates NotifyPropertyChanged events when set is called.
	/// Note: The event is running with UiContext.RunOn unless InterfaceImplConfig.UseUiContextRunOnForEvents is set to false.
	/// </summary>
	[Extension(typeof (INotifyPropertyChanged))]
	internal class NotifyPropertyChangedExtension : AbstractInterceptorExtension
	{
		// The "backing" event
		private event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		///     Register methods and setter
		/// </summary>
		/// <param name="interceptor"></param>
		public override void Initialize(IExtensibleInterceptor interceptor)
		{
			base.Initialize(interceptor);

			// Map the default add / remove handler methods
			interceptor.RegisterMethod($"add_{nameof(PropertyChanged)}", AddPropertyChanged);
			interceptor.RegisterMethod($"remove_{nameof(PropertyChanged)}", RemovePropertyChanged);

			// This is not default, but used by EventOberservable (Dapplo.Utils)
			interceptor.RegisterMethod($"invoke_{nameof(PropertyChanged)}", InvokePropertyChanged);
			// Register the NotifyPropertyChangedSetter as a last setter, it will call the NPC event
			interceptor.RegisterSetter((int) CallOrder.Last, NotifyPropertyChangedSetter);
		}

		/// <summary>
		///     This is the logic which is called to invoke the event.
		/// </summary>
		/// <param name="sender">object</param>
		/// <param name="eventArgs">PropertyChangedEventArgs</param>
		private void InvokePropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
		{
			PropertyChanged?.Invoke(sender, eventArgs);
		}

		#region registered methods
		/// <summary>
		///     This creates a NPC event if the values are changed
		/// </summary>
		/// <param name="setInfo">SetInfo with all the set call information</param>
		private void NotifyPropertyChangedSetter(SetInfo setInfo)
		{
			// Fast exit when no listeners.
			if (PropertyChanged == null)
			{
				return;
			}
			// Create the event if the property changed
			if (!setInfo.HasOldValue || !Equals(setInfo.NewValue, setInfo.OldValue))
			{
				// Find the real property name
				var propertyName = setInfo.Interceptor.PropertyNameFor(setInfo.PropertyName);
				if (propertyName == null)
				{
					return;
				}
				var propertyChangedEventArgs = new PropertyChangedEventArgs(propertyName);
				InvokePropertyChanged(setInfo.Interceptor, propertyChangedEventArgs);
			}
		}

		/// <summary>
		///     This is the logic which is called to invoke the event.
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void InvokePropertyChanged(MethodCallInfo methodCallInfo)
		{
			InvokePropertyChanged(methodCallInfo.Arguments[0], (PropertyChangedEventArgs)methodCallInfo.Arguments[1]);
		}

	
		/// <summary>
		///     This is the logic which is called when the PropertyChanged event is registered.
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void AddPropertyChanged(MethodCallInfo methodCallInfo)
		{
			// Add the parameters which should contain the event handler
			PropertyChanged += (PropertyChangedEventHandler)methodCallInfo.Arguments[0];
		}

		/// <summary>
		///     This is the logic which is called when the PropertyChanged event is unregistered.
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void RemovePropertyChanged(MethodCallInfo methodCallInfo)
		{
			// Remove the handler via the parameter which should contain the event handler
			PropertyChanged -= (PropertyChangedEventHandler) methodCallInfo.Arguments[0];
		}
		#endregion
	}
}