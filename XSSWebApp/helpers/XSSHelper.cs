using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace XSSWebApp.helpers
{
    /// <summary>
    /// Helper class for dealing with Cross-Site scripting
    /// </summary>
    //public static class XSSHelper
    //{
    //    private static readonly HtmlSanitizer _htmlSanitizer;
    //    private static List<Type> TypesToIgnore => new List<Type>
    //    {
    //        typeof(int),
    //        typeof(long),
    //        typeof(decimal),
    //        typeof(float),
    //        typeof(double),
    //        typeof(Guid),
    //        typeof(List<int>),
    //        typeof(List<long>),
    //        typeof(List<decimal>),
    //        typeof(List<float>),
    //        typeof(List<double>),
    //        typeof(List<Guid>),
    //    };

    //    private static List<Type> TypesForCollections => new List<Type>
    //    {
    //        typeof(List<>),
    //        typeof(IEnumerable<>),
    //        typeof(ICollection<>),
    //        typeof(IList<>)
    //    };

    //    static XSSHelper()
    //    {
    //        _htmlSanitizer = new HtmlSanitizer();
    //    }

    //    public static bool CheckXSSInString(object value, Action<object> updateValueAction = null)
    //    {
    //        var type = value.GetType();
    //        if (type == typeof(string) && value != null)
    //        {
    //            string sanitizedValue = Sanitize(value.ToString());

    //            if (sanitizedValue.ToString().Length < value.ToString().Length)
    //            {
    //                return true;
    //            }
    //            else
    //            {
    //                updateValueAction?.Invoke(sanitizedValue);
    //            }
    //        }

    //        return false;
    //    }

    //    public static bool CheckXSSInProperties(object objectValue, object[] parentAttritubes = null, Action<object> updateValueAction = null, Action actionIfExistsXSS = null, bool onlySanitize = false)
    //    {
    //        if (objectValue == null) return false;

    //        var type = objectValue.GetType();

    //        if (TypesToIgnore.Contains(type))
    //            return false;

    //        if (type == typeof(string))
    //        {
    //            var allowHtmlAttr = parentAttritubes?.SingleOrDefault(x => x.GetType() == typeof(AllowHtmlAttribute));
    //            if (allowHtmlAttr != null || onlySanitize)
    //            {
    //                objectValue = Sanitize(objectValue.ToString());
    //            }
    //            else if (CheckXSSInString(objectValue, (object newValue) => objectValue = newValue))
    //            {
    //                actionIfExistsXSS?.Invoke();
    //                return true;
    //            }

    //            updateValueAction?.Invoke(objectValue);
    //        }
    //        else if (type.IsGenericType && TypesForCollections.Contains(type.GetGenericTypeDefinition()))
    //        {
    //            foreach (var item in objectValue as IEnumerable<object>)
    //            {
    //                if (CheckXSSInProperties(objectValue: item, actionIfExistsXSS: actionIfExistsXSS, onlySanitize: onlySanitize))
    //                    return true;
    //            }
    //        }
    //        else if (type.GetProperties().Length > 0)
    //        {
    //            foreach (var prop in objectValue.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
    //            {
    //                try
    //                {
    //                    var value = prop.GetValue(objectValue);
    //                    var attritubes = prop.GetCustomAttributes(true);
    //                    if (CheckXSSInProperties(objectValue: value, parentAttritubes: attritubes,
    //                        updateValueAction: (object newValue) => prop.SetValue(objectValue, newValue), actionIfExistsXSS, onlySanitize: onlySanitize))
    //                        return true;
    //                }
    //                catch (Exception ex)
    //                {
    //                }
    //            }

    //            updateValueAction?.Invoke(objectValue);
    //        }

    //        return false;
    //    }

    //    public static string Sanitize(string value) => _htmlSanitizer.Sanitize(value);

    //}

    /// <summary>
    /// Helper class for dealing with Cross-Site scripting
    /// </summary>
    public static class XSSHelper
    {
        private static readonly HtmlSanitizer _htmlSanitizer;
        private static List<Type> TypesToIgnore => new List<Type>
        {
            typeof(int),
            typeof(long),
            typeof(decimal),
            typeof(float),
            typeof(double),
            typeof(Guid),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(List<int>),
            typeof(List<long>),
            typeof(List<decimal>),
            typeof(List<float>),
            typeof(List<double>),
            typeof(List<Guid>),
            typeof(List<DateTime>),
            typeof(List<DateTimeOffset>),
        };

        private static List<Type> TypesForCollections => new List<Type>
        {
            typeof(List<>),
            typeof(IEnumerable<>),
            typeof(ICollection<>),
            typeof(IList<>)
        };

        static XSSHelper()
        {
            _htmlSanitizer = new HtmlSanitizer();
        }

        private static bool CheckXSSInString(object value, Action<object> updateValueAction = null)
        {
            var type = value.GetType();
            if (type == typeof(string) && value != null)
            {
                string sanitizedValue = Sanitize(value.ToString());

                if (sanitizedValue.ToString().Length < value.ToString().Length)
                {
                    return true;
                }
                else
                {
                    updateValueAction?.Invoke(sanitizedValue);
                }
            }

            return false;
        }

        public static bool CheckXSSInProperties(object objectValue, object[] parentAttritubes = null, Action<object> updateValueAction = null, Action actionIfExistsXSS = null, bool onlySanitize = false)
        {
            Queue<PropInfo> queue = new Queue<PropInfo>();
            queue.Enqueue(new PropInfo(objectValue, parentAttritubes, updateValueAction));

            while (queue.Count > 0)
            {
                var currentPropInfo = queue.Dequeue();
                var currentObject = currentPropInfo.Value;
                var currentAttributes = currentPropInfo.Attributes;

                if (currentObject == null)
                    continue;

                var type = currentObject.GetType();

                if (TypesToIgnore.Contains(type))
                    continue;

                if (type == typeof(string))
                {
                    var allowHtmlAttr = currentAttributes?.SingleOrDefault(x => x.GetType() == typeof(AllowHtmlAttribute));
                    if (allowHtmlAttr != null || onlySanitize)
                    {
                        currentObject = Sanitize(currentObject.ToString());
                    }
                    else if (CheckXSSInString(currentObject, (object newValue) => currentObject = newValue))
                    {
                        actionIfExistsXSS?.Invoke();
                        return true;
                    }

                    currentPropInfo.UpdateValueAction?.Invoke(currentObject);
                }
                else if (type.IsGenericType && TypesForCollections.Contains(type.GetGenericTypeDefinition()))
                {
                    foreach (var item in currentObject as IEnumerable<object>)
                    {
                        //if (CheckXSSInProperties(objectValue: item, actionIfExistsXSS: actionIfExistsXSS, onlySanitize: onlySanitize))
                        //    return true;
                        queue.Enqueue(new PropInfo(item, parentAttritubes));
                    }
                }
                else if (type.GetProperties().Length > 0)
                {
                    foreach (var prop in currentObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        try
                        {
                            var value = prop.GetValue(currentObject);
                            var attritubes = prop.GetCustomAttributes(true);
                            queue.Enqueue(new PropInfo(value, attritubes, (object newValue) => prop.SetValue(currentObject, newValue)));
                            //if (CheckXSSInProperties(objectValue: value, parentAttritubes: attritubes, updateValueAction: (object newValue) => prop.SetValue(currentObject, newValue), actionIfExistsXSS, onlySanitize: onlySanitize))
                            //return true;
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    currentPropInfo.UpdateValueAction?.Invoke(currentObject);
                }
            }

            return false;
        }

        public static string Sanitize(string value) => _htmlSanitizer.Sanitize(value);

        class PropInfo
        {
            public object Value { get; set; }

            public object[] Attributes { get; set; }

            public Action<object> UpdateValueAction { get; set; }

            public PropInfo(object value, object[] attributes)
                : this(value, null, null)
            { }

            public PropInfo(object value, object[] attributes, Action<object> updateValueAction)
            {
                Value = value;
                Attributes = attributes;
                UpdateValueAction = updateValueAction;
            }
        }

    }

}