// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Structs;

using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Logging;
using FFXIVClientStructs.Interop;
using Newtonsoft.Json.Linq;
using ScreenshotStudio.GameData;
using ScreenshotStudio.Services;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static FFXIVClientStructs.FFXIV.Client.UI.Info.InfoProxyFriendList;
using static ScreenshotStudio.Structs.StructViewModelBase;

public static class StructFieldBindCache
{
	private static readonly Dictionary<Type, List<FieldBind>> FieldBindsLookup = new();
	private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> FieldInfosLookup = new();

	private static readonly ILogger Log = Logging.ForContext(typeof(StructFieldBindCache));

	public static List<FieldBind> GetBinds(Type viewModelType, Type structType)
	{
		if (FieldBindsLookup.ContainsKey(viewModelType))
		{
			return FieldBindsLookup[viewModelType];
		}
		else
		{
			List<FieldBind> fieldBinds = new();

			FieldInfo[] fields = structType.GetFields();
			PropertyInfo[] properties = viewModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			foreach (FieldInfo field in fields)
			{
				string fieldName = field.Name;

				PropertyInfo? foundProperty = null;
				foreach (PropertyInfo property in properties)
				{
					if (property.Name == fieldName)
					{
						if (foundProperty != null)
							Log.Warning($"Duplicate property found: {fieldName} in {viewModelType}");

						foundProperty = property;
					}
				}

				if (foundProperty == null)
				{
					Log.Warning($"Property not found: {fieldName} in {viewModelType}, found {properties.Length} properties.");
					continue;
				}

				fieldBinds.Add(new(field, foundProperty));
			}

			FieldBindsLookup.Add(viewModelType, fieldBinds);
			return fieldBinds;
		}
	}

	public static Dictionary<string, FieldInfo> GetFields(Type structType)
	{
		if (FieldInfosLookup.ContainsKey(structType))
		{
			return FieldInfosLookup[structType];
		}
		else
		{
			Dictionary<string, FieldInfo> fieldInfos = new();

			FieldInfo[] fields = structType.GetFields();
			foreach (FieldInfo field in fields)
			{
				fieldInfos.Add(field.Name, field);
			}

			FieldInfosLookup.Add(structType, fieldInfos);
			return fieldInfos;
		}
	}
}

public abstract class StructViewModelBase : INotifyPropertyChanged, IDisposable
{
	private readonly List<FieldBind> fieldBinds;
	private readonly Dictionary<string, FieldInfo> fieldLookup = new();

	private bool isDisposed = false;
	private object? structObject = null;

	public StructViewModelBase()
	{
		Type thisType = this.GetType();
		this.Log = Logging.ForContext(thisType);
		this.fieldBinds = StructFieldBindCache.GetBinds(thisType, this.GetModelType());
		this.fieldLookup = StructFieldBindCache.GetFields(this.GetModelType());

		StructViewModelService.Register(this);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public bool IsDisposed => this.isDisposed;
	public ILogger Log { get; init; }
	public ServiceManager Services => ServiceManager.Instance;
	public virtual object? StructObject => this.structObject;

	public void Dispose()
	{
		this.isDisposed = true;
		StructViewModelService.Unregister(this);
	}

	public virtual unsafe void Tick()
	{
		try
		{
			if (this.IsDisposed)
				return;

			foreach (FieldBind bind in this.fieldBinds)
			{
				object? fieldVal = this.GetValue(bind.Field);

				if (fieldVal == null)
					continue;

				if (!fieldVal.Equals(bind.LastValue))
				{
					bind.LastValue = fieldVal;

					if (typeof(StructViewModelBase).IsAssignableFrom(bind.Property.PropertyType))
					{
						// if this is a view model, update it
						StructViewModelBase? vm = bind.Property.GetValue(this) as StructViewModelBase;
						if (vm == null)
						{
							this.Log.Error($"No view model in Property: {bind.Property.Name} for View Model: {this.GetType()}");
							continue;
						}

						if (fieldVal is IntPtr address)
						{
							vm.SetAddress(address);
						}
						else if(fieldVal is Pointer pointer)
						{
							vm.SetAddress((IntPtr)Pointer.Unbox(pointer));
						}
						else
						{
							vm.SetStruct(fieldVal);
						}
					}

					this.NotifyPropertyChanged(bind.Property.Name);
				}
			}
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, $"Error ticking struct view model {this}");
		}
	}

	public virtual void SetStruct(object? model)
	{
		this.structObject = model;
	}

	public abstract void SetAddress(IntPtr address);

	public abstract Type GetModelType();

	protected void SetValue(object? value, [CallerMemberName] string fieldName = "")
	{
		if (!this.fieldLookup.TryGetValue(fieldName, out var field))
		{
			this.Log.Error($"Attempt to set struct value for missing field: {fieldName}");
			return;
		}

		this.SetValue(field, value);
	}

	protected virtual void SetValue(FieldInfo field, object? value)
	{
		this.Log.Information($"SetValue {field.Name} -> {value}");
		field.SetValue(this.StructObject, value);
	}

	protected virtual object? GetValue(FieldInfo field)
	{
		if (this.StructObject == null)
		{
			////this.Log.Error($"Attempt to get value without a struct: {field.Name}");
			return default;
		}

		return field.GetValue(this.StructObject);
	}

	protected object? GetValue([CallerMemberName] string fieldName = "")
	{
		if (!this.fieldLookup.TryGetValue(fieldName, out var field))
		{
			////this.Log.Error($"Attempt to get struct value for missing field: {fieldName}");
			return default;
		}

		return this.GetValue(field);
	}

	protected TValue? GetValue<TValue>([CallerMemberName] string fieldName = "")
	{
		return (TValue?)this.GetValue(fieldName);
	}

	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}

	public class FieldBind
	{
		public readonly FieldInfo Field;
		public readonly PropertyInfo Property;

		public object? LastValue;

		public FieldBind(FieldInfo field, PropertyInfo property)
		{
			this.Field = field;
			this.Property = property;
		}
	}
}

public abstract class StructViewModelBase<T> : StructViewModelBase
	where T : unmanaged
{
	public IntPtr? Address { get; private set; }

	public unsafe T* StructPointer => (T*)(IntPtr)this.Address!;

	public override sealed Type GetModelType() => typeof(T);

	public override void SetAddress(IntPtr address)
	{
		if (this.Address == address)
			return;

		this.Log.Information($"SetAddress {this.Address} -> {address}");
		this.Address = address;
	}

	public override void SetStruct(object? structObject)
	{
		if (this.Address != null)
		{
			this.Log.Warning($"Attempt to set struct for struct view model with pointer address");
			return;
		}

		base.SetStruct(structObject);
	}

	protected unsafe override object? GetValue(FieldInfo field)
	{
		if (this.Address != null)
		{
			// This reads from a copy, this is ok I guess?
			return field.GetValue(*this.StructPointer);
		}
		else
		{
			return base.GetValue(field);
		}
	}

	protected unsafe override void SetValue(FieldInfo field, object? value)
	{
		if (this.Address != null)
		{
			// this writes to a copy, obviously useless
			field.SetValue(*this.StructPointer, value);
		}
		else
		{
			base.SetValue(field, value);
		}

		this.NotifyPropertyChanged(field.Name);
	}
}