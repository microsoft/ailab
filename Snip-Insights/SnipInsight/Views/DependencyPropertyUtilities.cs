// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Windows;

namespace SnipInsight.Views
{
    public static class DependencyPropertyUtilities
    {
        #region Register

        //
        //  Standard Register
        //

        public static DependencyProperty Register<TOBJ, TPROP>(string name)
            where TOBJ : DependencyObject
        {
            return DependencyProperty.Register(name, typeof(TPROP), typeof(TOBJ));
        }

        public static DependencyProperty Register<TOBJ, TPROP>(string name, TPROP defaultValue)
            where TOBJ : DependencyObject
        {
            return DependencyProperty.Register(name, typeof(TPROP), typeof(TOBJ), CreatePropertyMetadata(defaultValue));
        }

        public static DependencyProperty Register<TOBJ, TPROP>(string name, TPROP defaultValue, PropertyChangedCallback propertyChangedCallback)
            where TOBJ : DependencyObject
        {
            return DependencyProperty.Register(name, typeof(TPROP), typeof(TOBJ), CreatePropertyMetadata(defaultValue, propertyChangedCallback));
        }

        public static DependencyProperty Register<TOBJ, TPROP>(string name, TPROP defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback)
            where TOBJ : DependencyObject
        {
            return DependencyProperty.Register(name, typeof(TPROP), typeof(TOBJ), CreatePropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback));
        }

        //
        //  On Change with New Value Only
        //

        public static DependencyProperty Register<TOBJ, TPROP>(string name, TPROP defaultValue, Action<TOBJ, TPROP> onChange)
            where TOBJ : DependencyObject
        {
            return Register<TOBJ, TPROP>(name, defaultValue, CreatePropertyChangedCallback(onChange));
        }

        public static DependencyProperty Register<TOBJ, TPROP>(string name, TPROP defaultValue, Action<TOBJ, TPROP> onChange, Func<TPROP, TPROP> coerceValue)
            where TOBJ : DependencyObject
        {
            return Register<TOBJ, TPROP>(name, defaultValue, CreatePropertyChangedCallback(onChange), CreateCoerceValueCallback(coerceValue));
        }

        //
        //  On Change with New/Old Values
        //

        public static DependencyProperty Register<TOBJ, TPROP>(string name, TPROP defaultValue, Action<TOBJ, TPROP, TPROP> onChange)
            where TOBJ : DependencyObject
        {
            return Register<TOBJ, TPROP>(name, defaultValue, CreatePropertyChangedCallback(onChange));
        }

        public static DependencyProperty Register<TOBJ, TPROP>(string name, TPROP defaultValue, Action<TOBJ, TPROP, TPROP> onChange, Func<TPROP, TPROP> coerceValue)
            where TOBJ : DependencyObject
        {
            return Register<TOBJ, TPROP>(name, defaultValue, CreatePropertyChangedCallback(onChange), CreateCoerceValueCallback(coerceValue));
        }

        #endregion

        #region PropertyMetadata

        public static PropertyMetadata CreatePropertyMetadata<TPROP>(TPROP defaultValue)
        {
            return new PropertyMetadata(defaultValue);
        }

        public static PropertyMetadata CreatePropertyMetadata<TPROP>(TPROP defaultValue, PropertyChangedCallback propertyChangedCallback)
        {
            return new PropertyMetadata(defaultValue, propertyChangedCallback);
        }

        public static PropertyMetadata CreatePropertyMetadata<TPROP>(TPROP defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback)
        {
            return new PropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback);
        }

        public static PropertyMetadata CreatePropertyMetadata<TOBJ, TPROP>(TPROP defaultValue, Action<TOBJ, TPROP> onChange)
            where TOBJ : DependencyObject
        {
            return new PropertyMetadata(defaultValue, CreatePropertyChangedCallback<TOBJ, TPROP>(onChange));
        }

        public static PropertyMetadata CreatePropertyMetadata<TOBJ, TPROP>(TPROP defaultValue, Action<TOBJ, TPROP, TPROP> onChange)
            where TOBJ : DependencyObject
        {
            return new PropertyMetadata(defaultValue, CreatePropertyChangedCallback<TOBJ, TPROP>(onChange));
        }

        public static PropertyMetadata CreatePropertyMetadata<TOBJ, TPROP>(TPROP defaultValue, Action<TOBJ, TPROP> onChange, Func<TPROP, TPROP> coerceValue)
            where TOBJ : DependencyObject
        {
            return new PropertyMetadata(defaultValue, CreatePropertyChangedCallback<TOBJ, TPROP>(onChange), CreateCoerceValueCallback(coerceValue));
        }

        public static PropertyMetadata CreatePropertyMetadata<TOBJ, TPROP>(TPROP defaultValue, Action<TOBJ, TPROP, TPROP> onChange, Func<TPROP, TPROP> coerceValue)
            where TOBJ : DependencyObject
        {
            return new PropertyMetadata(defaultValue, CreatePropertyChangedCallback<TOBJ, TPROP>(onChange), CreateCoerceValueCallback(coerceValue));
        }

        #endregion

        #region PropertyChangedCallback

        public static PropertyChangedCallback CreatePropertyChangedCallback<TOBJ, TPROP>(Action<TOBJ, TPROP> onChange)
            where TOBJ : DependencyObject
        {
            PropertyChangedCallback callback = null;

            if (onChange != null)
            {
                Action<DependencyObject, DependencyPropertyChangedEventArgs> staticOnChange = (d, e) =>
                {
                    TOBJ obj = d as TOBJ;

                    if (obj != null)
                        onChange(obj, (TPROP)e.NewValue);
                };

                callback = new PropertyChangedCallback(staticOnChange);
            }

            return callback;
        }

        public static PropertyChangedCallback CreatePropertyChangedCallback<TOBJ, TPROP>(Action<TOBJ, TPROP, TPROP> onChange)
            where TOBJ : DependencyObject
        {
            PropertyChangedCallback callback = null;

            if (onChange != null)
            {
                Action<DependencyObject, DependencyPropertyChangedEventArgs> staticOnChange = (d, e) =>
                {
                    TOBJ obj = d as TOBJ;

                    if (obj != null)
                        onChange(obj, (TPROP)e.NewValue, (TPROP)e.OldValue);
                };

                callback = new PropertyChangedCallback(staticOnChange);
            }

            return callback;
        }

        #endregion

        #region CoerceValueCallback

        public static CoerceValueCallback CreateCoerceValueCallback<TPROP>(Func<TPROP, TPROP> coerceValue)
        {
            CoerceValueCallback callback = null;

            if (coerceValue != null)
            {
                callback = new CoerceValueCallback((o, p) => { return coerceValue((TPROP)p); });
            }

            return callback;
        }

        public static CoerceValueCallback CreateCoerceValueCallback<TPROP>(Func<object, TPROP> coerceValue)
        {
            CoerceValueCallback callback = null;

            if (coerceValue != null)
            {
                callback = new CoerceValueCallback((o, p) => { return coerceValue(p); });
            }

            return callback;
        }

        #endregion
    }
}
