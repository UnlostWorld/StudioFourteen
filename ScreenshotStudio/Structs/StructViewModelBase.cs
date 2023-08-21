// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Structs;

using Dalamud.Game.ClientState.JobGauge.Enums;
using ScreenshotStudio.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static ScreenshotStudio.Structs.StructViewModelBase;

public static class StructFieldBindCache
{
	private static readonly Dictionary<Type, List<FieldBind>> FieldBindsLookup = new();
	private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> FieldInfosLookup = new();

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

	public StructViewModelBase()
	{
		Type thisType = this.GetType();
		this.Log = Serilog.Log.ForContext(thisType);
		this.fieldBinds = StructFieldBindCache.GetBinds(thisType, this.GetModelType());
		this.fieldLookup = StructFieldBindCache.GetFields(this.GetModelType());

		StructViewModelService.Register(this);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public bool IsDisposed => this.isDisposed;
	public ILogger Log { get; init; }
	public object? Struct { get; private set; }

	public void Dispose()
	{
		this.isDisposed = true;
		StructViewModelService.Unregister(this);
	}

	public virtual void Tick()
	{
		if (this.IsDisposed)
			return;

		if (this.Struct == null)
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

					vm.SetModel(fieldVal);
				}
				else
				{
					bind.Property.SetValue(this, fieldVal);
				}

				this.NotifyPropertyChanged(bind.Property.Name);
			}
		}
	}

	public void SetModel(object? model)
	{
		this.Struct = model;
	}

	public abstract Type GetModelType();

	protected virtual void SetValue(object? value, [CallerMemberName] string fieldName = "")
	{
		if (!this.fieldLookup.TryGetValue(fieldName, out var field))
		{
			this.Log.Error($"Attempt to set struct value for missing field: {fieldName}");
			return;
		}

		field.SetValue(this.Struct, value);
	}

	protected virtual object? GetValue(FieldInfo field)
	{
		if (this.Struct == null)
		{
			this.Log.Error($"Attempt to get struct value without a model: {field.Name}");
			return default;
		}

		return field.GetValue(this.Struct);
	}

	protected object? GetValue([CallerMemberName] string fieldName = "")
	{
		if (!this.fieldLookup.TryGetValue(fieldName, out var field))
		{
			this.Log.Error($"Attempt to get struct value for missing field: {fieldName}");
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
	public new T Struct => (T)base.Struct!;
	public override sealed Type GetModelType() => typeof(T);
}

public unsafe abstract class StructPtrViewModelBase<T> : StructViewModelBase<T>
	where T : unmanaged
{
	public StructPtrViewModelBase(IntPtr ptr)
	{
		this.SetAddress(ptr);
	}

	public IntPtr Address { get; private set; }

	public void SetAddress(IntPtr address)
	{
		this.Address = address;

		T model = Marshal.PtrToStructure<T>(address);
		this.SetModel(model);
	}

	public override void Tick()
	{
		this.SetAddress(this.Address);
		base.Tick();
	}
}
