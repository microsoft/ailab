using Microsoft.Xaml.Interactions.Core;
using Microsoft.Xaml.Interactivity;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Speaker.Recorder.UWP.Controls
{
    public class ChangeAttachedPropertyAction : DependencyObject, IAction
    {
        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register(
            nameof(PropertyName),
            typeof(PropertyPath),
            typeof(ChangeAttachedPropertyAction),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(object),
            typeof(ChangeAttachedPropertyAction),
            new PropertyMetadata(null));

        public static readonly DependencyProperty TargetObjectProperty = DependencyProperty.Register(
            nameof(TargetObject),
            typeof(object),
            typeof(ChangeAttachedPropertyAction),
            new PropertyMetadata(null));

        public PropertyPath PropertyName
        {
            get
            {
                return (PropertyPath)this.GetValue(PropertyNameProperty);
            }
            set
            {
                this.SetValue(PropertyNameProperty, value);
            }
        }

        public object Value
        {
            get
            {
                return this.GetValue(ValueProperty);
            }
            set
            {
                this.SetValue(ValueProperty, value);
            }
        }

        public object TargetObject
        {
            get
            {
                return this.GetValue(TargetObjectProperty);
            }
            set
            {
                this.SetValue(TargetObjectProperty, value);
            }
        }

        public object Execute(object sender, object parameter)
        {
            var targetObject = sender;
            if (this.ReadLocalValue(ChangePropertyAction.TargetObjectProperty) != DependencyProperty.UnsetValue)
            {
                targetObject = this.TargetObject;
            }

            if (targetObject == null || this.PropertyName == null)
            {
                return false;
            }

            this.UpdatePropertyValue(targetObject);
            return true;
        }

        private void UpdatePropertyValue(object targetObject)
        {
            var propertyNamePaths = this.PropertyName.Path.Split('.');
            var className = propertyNamePaths[0];
            var propertyName = propertyNamePaths[propertyNamePaths.Length - 1];
            // At this moment only works with types in Windows.UI.Xaml.Controls namespace
            Type targetType = typeof(Grid).Assembly.GetType("Windows.UI.Xaml.Controls." + className);

            var setMethod = targetType.GetRuntimeMethods().FirstOrDefault(x => x.Name == $"Set{propertyName}");
            if(setMethod == null)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "Property {0} defined by type {1} does not expose a set method and therefore cannot be modified.",
                    propertyName,
                    className));
            }

            var propertyType = setMethod.GetParameters()[1].ParameterType;

            Exception innerException = null;
            try
            {
                object result = null;
                string valueAsString = null;
                TypeInfo propertyTypeInfo = propertyType.GetTypeInfo();
                if (this.Value == null)
                {
                    // The result can be null if the type is generic (nullable), or the default value of the type in question
                    result = propertyTypeInfo.IsValueType ? Activator.CreateInstance(propertyType) : null;
                }
                else if (propertyTypeInfo.IsAssignableFrom(this.Value.GetType().GetTypeInfo()))
                {
                    result = this.Value;
                }
                else
                {
                    valueAsString = this.Value.ToString();
                    result = propertyTypeInfo.IsEnum ? Enum.Parse(propertyType, valueAsString, false) :
                        TypeConverterHelper.Convert(valueAsString, propertyType.FullName);
                }

                setMethod.Invoke(null, new[] { targetObject, result });
            }
            catch (FormatException e)
            {
                innerException = e;
            }
            catch (ArgumentException e)
            {
                innerException = e;
            }

            if (innerException != null)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "Cannot assign value of type {0} to property {1} of type {2}. The {1} property can be assigned only values of type {2}.",
                    this.Value != null ? this.Value.GetType().Name : "null",
                    this.PropertyName,
                    propertyType.Name),
                    innerException);
            }
        }
    }
}