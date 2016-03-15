﻿//******************************************************************************************************
//  DataContext.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/01/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using GSF.Collections;
using GSF.Data;
using GSF.Data.Model;
using GSF.Reflection;
using GSF.Web.Templating;
using RazorEngine.Templating;

// ReSharper disable once StaticMemberInGenericType
namespace GSF.Web.Model
{
    /// <summary>
    /// Defines a default data context object based on embedded resources.
    /// </summary>
    public class DataContext : DataContext<CSharpEmbeddedResource>
    {
        /// <summary>
        /// Creates a new <see cref="DataContext"/>.
        /// </summary>
        /// <param name="connection"><see cref="AdoDataConnection"/> to use; defaults to a new connection.</param>
        /// <param name="disposeConnection">Set to <c>true</c> to dispose the provided <paramref name="connection"/>.</param>
        /// <param name="exceptionHandler">Delegate to handle exceptions.</param>
        public DataContext(AdoDataConnection connection = null, bool disposeConnection = false, Action<Exception> exceptionHandler = null) :
            base(connection, disposeConnection, exceptionHandler)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DataContext"/> using the specified <paramref name="settingsCategory"/>.
        /// </summary>
        /// <param name="settingsCategory">Setting category that contains the connection settings.</param>
        /// <param name="exceptionHandler">Delegate to handle exceptions.</param>
        public DataContext(string settingsCategory, Action<Exception> exceptionHandler = null) : 
            base(settingsCategory, exceptionHandler)
        {
        }
    }

    /// <summary>
    /// Defines a data context for modeled tables.
    /// </summary>
    /// <typeparam name="TLanguage"><see cref="LanguageConstraint"/> type to use for Razor views.</typeparam>
    public class DataContext<TLanguage> : IDisposable where TLanguage : LanguageConstraint, new()
    {
        #region [ Members ]

        // Fields
        private AdoDataConnection m_connection;
        private readonly Dictionary<Type, object> m_tableOperations;
        private readonly Action<Exception> m_exceptionHandler;
        private readonly Dictionary<string, Tuple<string, string>> m_fieldValidationParameters;
        private readonly List<Tuple<string, string>> m_fieldValueInitializers; 
        private readonly List<string> m_definedDateFields; 
        private readonly string m_settingsCategory;
        private readonly bool m_disposeConnection;
        private string m_initialFocusField;
        private string m_addDateFieldTemplate;
        private string m_addInputFieldTemplate;
        private string m_addTextAreaFieldTemplate;
        private string m_addSelectFieldTemplate;
        private string m_addCheckBoxFieldTemplate;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataContext{TLanguage}"/>.
        /// </summary>
        /// <param name="connection"><see cref="AdoDataConnection"/> to use; defaults to a new connection.</param>
        /// <param name="disposeConnection">Set to <c>true</c> to dispose the provided <paramref name="connection"/>.</param>
        /// <param name="exceptionHandler">Delegate to handle exceptions.</param>
        public DataContext(AdoDataConnection connection = null, bool disposeConnection = false, Action<Exception> exceptionHandler = null)
        {
            m_connection = connection;
            m_tableOperations = new Dictionary<Type, object>();
            m_exceptionHandler = exceptionHandler;
            m_fieldValidationParameters = new Dictionary<string, Tuple<string, string>>();
            m_fieldValueInitializers = new List<Tuple<string, string>>();
            m_definedDateFields = new List<string>();
            m_settingsCategory = "systemSettings";
            m_disposeConnection = disposeConnection || connection == null;
        }

        /// <summary>
        /// Creates a new <see cref="DataContext{TLanguage}"/> using the specified <paramref name="settingsCategory"/>.
        /// </summary>
        /// <param name="settingsCategory">Setting category that contains the connection settings.</param>
        /// <param name="exceptionHandler">Delegate to handle exceptions.</param>
        public DataContext(string settingsCategory, Action<Exception> exceptionHandler = null)
        {
            m_tableOperations = new Dictionary<Type, object>();
            m_exceptionHandler = exceptionHandler;
            m_fieldValidationParameters = new Dictionary<string, Tuple<string, string>>();
            m_fieldValueInitializers = new List<Tuple<string, string>>();
            m_definedDateFields = new List<string>();
            m_settingsCategory = settingsCategory;
            m_disposeConnection = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="AdoDataConnection"/> for this <see cref="DataContext{TLanguage}"/>.
        /// </summary>
        public AdoDataConnection Connection => m_connection ?? (m_connection = new AdoDataConnection(m_settingsCategory));

        /// <summary>
        /// Gets validation pattern and error message for rendered fields, if any.
        /// </summary>
        public Dictionary<string, Tuple<string, string>> FieldValidationParameters => m_fieldValidationParameters;

        /// <summary>
        /// Gets field value initializers, if any.
        /// </summary>
        public List<Tuple<string, string>> FieldValueInitializers => m_fieldValueInitializers;

        /// <summary>
        /// Gets defined date fields, if any.
        /// </summary>
        public List<string> DefinedDateFields => m_definedDateFields;

        /// <summary>
        /// Gets field name designated for initial focus.
        /// </summary>
        public string InitialFocusField => m_initialFocusField;

        /// <summary>
        /// Gets or sets the date field razor template file name.
        /// </summary>
        public string AddDateFieldTemplate
        {
            get
            {
                return m_addDateFieldTemplate ?? (m_addDateFieldTemplate = $"{RazorView<TLanguage>.TemplatePath}AddDateField.cshtml");
            }
            set
            {
                m_addDateFieldTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the input field razor template file name.
        /// </summary>
        public string AddInputFieldTemplate
        {
            get
            {
                return m_addInputFieldTemplate ?? (m_addInputFieldTemplate = $"{RazorView<TLanguage>.TemplatePath}AddInputField.cshtml");
            }
            set
            {
                m_addInputFieldTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the text area field razor template file name.
        /// </summary>
        public string AddTextAreaFieldTemplate
        {
            get
            {
                return m_addTextAreaFieldTemplate ?? (m_addTextAreaFieldTemplate = $"{RazorView<TLanguage>.TemplatePath}AddTextAreaField.cshtml");
            }
            set
            {
                m_addTextAreaFieldTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the select field razor template file name.
        /// </summary>
        public string AddSelectFieldTemplate
        {
            get
            {
                return m_addSelectFieldTemplate ?? (m_addSelectFieldTemplate = $"{RazorView<TLanguage>.TemplatePath}AddSelectField.cshtml");
            }
            set
            {
                m_addSelectFieldTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the check box field razor template file name.
        /// </summary>
        public string AddCheckBoxFieldTemplate
        {
            get
            {
                return m_addCheckBoxFieldTemplate ?? (m_addCheckBoxFieldTemplate = $"{RazorView<TLanguage>.TemplatePath}AddCheckBoxField.cshtml");
            }
            set
            {
                m_addCheckBoxFieldTemplate = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="DataContext{TLanguage}"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataContext{TLanguage}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_disposeConnection)
                            m_connection?.Dispose();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Gets the table operations for the specified modeled table <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <returns>Table operations for the specified modeled table <typeparamref name="TModel"/>.</returns>
        public TableOperations<TModel> Table<TModel>() where TModel : class, new()
        {
            return m_tableOperations.GetOrAdd(typeof(TModel), type => new TableOperations<TModel>(Connection, m_exceptionHandler)) as TableOperations<TModel>;
        }

        /// <summary>
        /// Gets the field name targeted as the primary label for the modeled table.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <returns>Field name targeted as the primary label for the modeled table.</returns>
        public string GetPrimaryLabelField<TModel>() where TModel : class, new()
        {
            PrimaryLabelAttribute primaryLabelAttribute;

            if (typeof(TModel).TryGetAttribute(out primaryLabelAttribute) && !string.IsNullOrWhiteSpace(primaryLabelAttribute.FieldName))
                return primaryLabelAttribute.FieldName;

            string[] fieldNames = Table<TModel>().GetFieldNames();
            return fieldNames.Length > 0 ? fieldNames[0] : "";
        }

        /// <summary>
        /// Gets the field name targeted to mark a record as deleted.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <returns>Field name targeted to mark a record as deleted.</returns>
        public string GetIsDeletedFlag<TModel>() where TModel : class, new()
        {
            IsDeletedFlagAttribute isDeletedFlagAttribute;

            if (typeof(TModel).TryGetAttribute(out isDeletedFlagAttribute) && !string.IsNullOrWhiteSpace(isDeletedFlagAttribute.FieldName))
                return isDeletedFlagAttribute.FieldName;

            return null;
        }

        /// <summary>
        /// Looks up proper user roles for paged based on modeled security in <see cref="RecordOperationsCache"/>.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <typeparam name="THub">SignalR hub that implements <see cref="IRecordOperationsHub"/>.</typeparam>
        /// <param name="viewBag">ViewBag for the current view.</param>
        /// <remarks>
        /// Typically used in paged view model scenarios and invoked by controller prior to view rendering.
        /// Security is controlled at hub level, so failure to call will not impact security but may result
        /// in screen enabling and/or showing controls that the user does not actually have access to and
        /// upon attempted use will result in a security error.
        /// </remarks>
        public void EstablishUserRolesForPage<TModel, THub>(dynamic viewBag) where TModel : class, new() where THub : IRecordOperationsHub, new()
        {
            RecordOperationsCache cache = s_recordOperationCaches.GetOrAdd(typeof(THub), type =>
            {
                using (THub hub = new THub())
                    return hub.RecordOperationsCache;
            });
            EstablishUserRolesForPage<TModel>(cache, viewBag);
        }

        /// <summary>
        /// Looks up proper user roles for paged based on modeled security in <see cref="RecordOperationsCache"/>.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <param name="cache">Data hub record operations cache.</param>
        /// <param name="viewBag">ViewBag for the current view.</param>
        /// <remarks>
        /// Typically used in paged view model scenarios and invoked by controller prior to view rendering.
        /// Security is controlled at hub level, so failure to call will not impact security but may result
        /// in screen enabling and/or showing controls that the user does not actually have access to and
        /// upon attempted use will result in a security error.
        /// </remarks>
        public void EstablishUserRolesForPage<TModel>(RecordOperationsCache cache, dynamic viewBag) where TModel : class, new()
        {
            // Get any authorized roles as defined in hub for key records operations of modeled table
            Tuple<string, string>[] recordOperations = cache.GetRecordOperations<TModel>();

            // Create a function to check if a method exists for operation - if none is defined, read-only access will be assumed (e.g. for a view)
            Func<RecordOperation, string> getRoles = operationType =>
            {
                Tuple<string, string> recordOperation = recordOperations[(int)operationType];
                return string.IsNullOrEmpty(recordOperation?.Item1) ? "" : recordOperation.Item2 ?? "";
            };

            viewBag.EditRoles = getRoles(RecordOperation.UpdateRecord);
            viewBag.AddNewRoles = getRoles(RecordOperation.AddNewRecord);
            viewBag.DeleteRoles = getRoles(RecordOperation.DeleteRecord);
        }

        /// <summary>
        /// Renders client-side configuration script for paged view model.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <typeparam name="THub">SignalR hub that implements <see cref="IRecordOperationsHub"/>.</typeparam>
        /// <param name="viewBag">ViewBag for the view.</param>
        /// <param name="defaultSortField">Default sort field name, defaults to first primary key field. Prefix field name with a minus, i.e., '-', to default to descending sort.</param>
        /// <param name="parentKeys">Primary keys values of the parent record to load.</param>
        /// <returns>Rendered paged view model configuration script.</returns>
        public string RenderViewModelConfiguration<TModel, THub>(dynamic viewBag, string defaultSortField = null, params object[] parentKeys) where TModel : class, new() where THub : IRecordOperationsHub, new()
        {
            RecordOperationsCache cache = s_recordOperationCaches.GetOrAdd(typeof(THub), type =>
            {
                using (THub hub = new THub())
                    return hub.RecordOperationsCache;
            });
            return RenderViewModelConfiguration<TModel>(cache, viewBag, defaultSortField, parentKeys);
        }

        /// <summary>
        /// Renders client-side configuration script for paged view model.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <param name="cache">Data hub record operations cache.</param>
        /// <param name="viewBag">ViewBag for the view.</param>
        /// <param name="defaultSortField">Default sort field name, defaults to first primary key field. Prefix field name with a minus, i.e., '-', to default to descending sort.</param>
        /// <param name="parentKeys">Primary keys values of the parent record to load.</param>
        /// <returns>Rendered paged view model configuration script.</returns>
        public string RenderViewModelConfiguration<TModel>(RecordOperationsCache cache, dynamic viewBag, string defaultSortField = null, params object[] parentKeys) where TModel : class, new()
        {
            StringBuilder javascript = new StringBuilder();
            string[] primaryKeyFields = Table<TModel>().GetPrimaryKeyFieldNames();
            string defaultSortAscending = "true";

            defaultSortField = defaultSortField ?? primaryKeyFields[0];

            // Handle case for desired default descending sort order
            if (defaultSortField[0] == '-')
            {
                defaultSortField = defaultSortField.Substring(1);
                defaultSortAscending = "false";
            }

            javascript.Append($@"// Configure view model
                viewModel.defaultSortField = ""{defaultSortField}"";
                viewModel.defaultSortAscending = {defaultSortAscending};
                viewModel.labelField = ""{GetPrimaryLabelField<TModel>()}"";
                viewModel.primaryKeyFields = [{primaryKeyFields.Select(fieldName => $"\"{fieldName}\"").ToDelimitedString(", ")}];
            ".FixForwardSpacing());

            string showDeletedValue = null;

            if (viewBag.IsDeletedField != null)
                showDeletedValue = (viewBag.ShowDeleted ?? false).ToString().ToLower();

            Func<string, string> toCamelCase = methodName => methodName == null ? null : $"{char.ToLower(methodName[0])}{methodName.Substring(1)}";

            // Get method names for records operations of modeled table
            Tuple<string, string>[] recordOperations = cache.GetRecordOperations<TModel>();
            string queryRecordCountMethod = toCamelCase(recordOperations[(int)RecordOperation.QueryRecordCount]?.Item1);
            string queryRecordsMethod = toCamelCase(recordOperations[(int)RecordOperation.QueryRecords]?.Item1);
            string deleteRecordMethod = toCamelCase(recordOperations[(int)RecordOperation.DeleteRecord]?.Item1);
            string createNewRecordMethod = toCamelCase(recordOperations[(int)RecordOperation.CreateNewRecord]?.Item1);
            string addNewRecordMethod = toCamelCase(recordOperations[(int)RecordOperation.AddNewRecord]?.Item1);
            string updateMethod = toCamelCase(recordOperations[(int)RecordOperation.UpdateRecord]?.Item1);

            string keyValues = null;

            if (parentKeys.Length > 0)
                keyValues = parentKeys.ToDelimitedString(", ");

            // If modeled table has IsDeletedField marker, the showDeleted parameter should come first in DataHub operations
            if (showDeletedValue != null)
                keyValues = keyValues != null ? $"{showDeletedValue}, {keyValues}" : showDeletedValue;

            if (!string.IsNullOrWhiteSpace(queryRecordCountMethod))
                javascript.Append($@"
                    viewModel.setQueryRecordCount(function () {{
                        return dataHub.{queryRecordCountMethod}({keyValues});
                    }});
                ".FixForwardSpacing());

            if (!string.IsNullOrWhiteSpace(queryRecordsMethod))
                javascript.Append($@"
                    viewModel.setQueryRecords(function (sortField, ascending, page, pageSize) {{
                        return dataHub.{queryRecordsMethod}({(keyValues == null ? "" : $"{keyValues}, ")}sortField, ascending, page, pageSize);
                    }});
                ".FixForwardSpacing());

            if (!string.IsNullOrWhiteSpace(deleteRecordMethod))
                javascript.Append($@"
                    viewModel.setDeleteRecord(function (keyValues) {{
                        return dataHub.{deleteRecordMethod}({Enumerable.Range(0, Table<TModel>().GetPrimaryKeyFieldNames().Length).Select(index => $"keyValues[{index}]").ToDelimitedString(", ")});
                    }});
                ".FixForwardSpacing());

            if (!string.IsNullOrWhiteSpace(createNewRecordMethod))
                javascript.Append($@"
                    viewModel.setNewRecord(function () {{
                        return dataHub.{createNewRecordMethod}();
                    }});
                ".FixForwardSpacing());

            if (!string.IsNullOrWhiteSpace(addNewRecordMethod))
                javascript.Append($@"
                    viewModel.setAddNewRecord(function (record) {{
                        return dataHub.{addNewRecordMethod}(record);
                    }});
                ".FixForwardSpacing());

            if (!string.IsNullOrWhiteSpace(updateMethod))
                javascript.Append($@"
                    viewModel.setUpdateRecord(function (record) {{
                        return dataHub.{updateMethod}(record);
                    }});
                ".FixForwardSpacing());

            return javascript.ToString();
        }

        /// <summary>
        /// Adds a new pattern based validation and option error message to a field.
        /// </summary>
        /// <param name="observableFieldReference">Observable field reference (from JS view model).</param>
        /// <param name="validationPattern">Reg-ex based validation pattern.</param>
        /// <param name="errorMessage">Optional error message to display when pattern fails.</param>
        public void AddFieldValidation(string observableFieldReference, string validationPattern, string errorMessage = null)
        {
            m_fieldValidationParameters[observableFieldReference] = new Tuple<string, string>(validationPattern, errorMessage);
        }

        /// <summary>
        /// Adds a new field value initializer.
        /// </summary>
        /// <param name="fieldName">Field name (as defined in model).</param>
        /// <param name="initialValue">Javascript based initial value for field.</param>
        public void AddFieldValueInitializer(string fieldName, string initialValue = null)
        {
            m_fieldValueInitializers.Add(new Tuple<string, string>(fieldName, initialValue ?? "\"\""));
        }

        /// <summary>
        /// Adds a new field value initializer based on modeled table field attributes.
        /// </summary>
        /// <param name="fieldName">Field name (as defined in model).</param>
        public void AddFieldValueInitializer<TModel>(string fieldName) where TModel : class, new()
        {
            InitialValueAttribute initialValueAttribute;

            if (Table<TModel>().TryGetFieldAttribute(fieldName, out initialValueAttribute))
                AddFieldValueInitializer(fieldName, initialValueAttribute.InitialValue);
        }

        /// <summary>
        /// Generates template based input date field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="TModel">Modeled table.</typeparam>
        /// <param name="fieldName">Field name for input date field.</param>
        /// <param name="inputType">Input field type, defaults to text.</param>
        /// <param name="fieldLabel">Label name for input date field, pulls from <see cref="LabelAttribute"/> if defined, otherwise defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for input field; defaults to date + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new date based input field based on modeled table field attributes.</returns>
        public string AddDateField<TModel>(string fieldName, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false) where TModel : class, new()
        {
            TableOperations<TModel> tableOperations = Table<TModel>();
            StringLengthAttribute stringLengthAttribute;
            RegularExpressionAttribute regularExpressionAttribute;

            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (tableOperations.TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            tableOperations.TryGetFieldAttribute(fieldName, out stringLengthAttribute);
            tableOperations.TryGetFieldAttribute(fieldName, out regularExpressionAttribute);

            if ((object)regularExpressionAttribute != null)
            {
                string observableReference;

                if (string.IsNullOrEmpty(groupDataBinding))
                    observableReference = $"viewModel.currentRecord().{fieldName}";
                else // "with: $root.connectionString"
                    observableReference = $"viewModel.{groupDataBinding.Substring(groupDataBinding.IndexOf('.') + 1)}";

                AddFieldValidation(observableReference, regularExpressionAttribute.Pattern, regularExpressionAttribute.ErrorMessage ?? "Invalid format.");
            }

            AddFieldValueInitializer<TModel>(fieldName);

            return AddDateField(fieldName, tableOperations.FieldHasAttribute<RequiredAttribute>(fieldName),
                stringLengthAttribute?.MaximumLength ?? 0, inputType, fieldLabel, fieldID, groupDataBinding, labelDataBinding, requiredDataBinding, customDataBinding, dependencyFieldName, toolTip, initialFocus);
        }

        /// <summary>
        /// Generates template based input date field based on specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name for input date field.</param>
        /// <param name="required">Determines if field name is required.</param>
        /// <param name="maxLength">Defines maximum input field length.</param>
        /// <param name="inputType">Input field type, defaults to text.</param>
        /// <param name="fieldLabel">Label name for input date field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for input field; defaults to date + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new date based input field based on specified parameters.</returns>
        public string AddDateField(string fieldName, bool required, int maxLength = 0, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false)
        {
            RazorView<TLanguage> addDateFieldTemplate = new RazorView<TLanguage>(AddDateFieldTemplate, m_exceptionHandler);
            DynamicViewBag viewBag = addDateFieldTemplate.ViewBag;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"date{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("MaxLength", maxLength);
            viewBag.AddValue("InputType", inputType ?? "text");
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("RequiredDataBinding", requiredDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            m_definedDateFields.Add(fieldName);

            return addDateFieldTemplate.Execute();
        }

        /// <summary>
        /// Generates template based input text field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="TModel">Modeled table.</typeparam>
        /// <param name="fieldName">Field name for input text field.</param>
        /// <param name="inputType">Input field type, defaults to text.</param>
        /// <param name="fieldLabel">Label name for input text field, pulls from <see cref="LabelAttribute"/> if defined, otherwise defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for input field; defaults to input + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new input field based on modeled table field attributes.</returns>
        public string AddInputField<TModel>(string fieldName, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false) where TModel : class, new()
        {
            TableOperations<TModel> tableOperations = Table<TModel>();
            StringLengthAttribute stringLengthAttribute;
            RegularExpressionAttribute regularExpressionAttribute;

            if (string.IsNullOrEmpty(inputType))
            {
                Type fieldType = tableOperations.GetFieldType(fieldName);

                if (IsNumericType(fieldType))
                    inputType = "number";
                else if (fieldType == typeof(DateTime))
                    inputType = "date";
            } 

            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (tableOperations.TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            tableOperations.TryGetFieldAttribute(fieldName, out stringLengthAttribute);
            tableOperations.TryGetFieldAttribute(fieldName, out regularExpressionAttribute);

            if ((object)regularExpressionAttribute != null)
            {
                string observableReference;

                if (string.IsNullOrEmpty(groupDataBinding))
                    observableReference = $"viewModel.currentRecord().{fieldName}";
                else // "with: $root.connectionString"
                    observableReference = $"viewModel.{groupDataBinding.Substring(groupDataBinding.IndexOf('.') + 1)}";

                AddFieldValidation(observableReference, regularExpressionAttribute.Pattern, regularExpressionAttribute.ErrorMessage ?? "Invalid format.");
            }

            AddFieldValueInitializer<TModel>(fieldName);

            return AddInputField(fieldName, tableOperations.FieldHasAttribute<RequiredAttribute>(fieldName),
                stringLengthAttribute?.MaximumLength ?? 0, inputType, fieldLabel, fieldID, groupDataBinding, labelDataBinding, requiredDataBinding, customDataBinding, dependencyFieldName, toolTip, initialFocus);
        }

        /// <summary>
        /// Generates template based input text field based on specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name for input text field.</param>
        /// <param name="required">Determines if field name is required.</param>
        /// <param name="maxLength">Defines maximum input field length.</param>
        /// <param name="inputType">Input field type, defaults to text.</param>
        /// <param name="fieldLabel">Label name for input text field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for input field; defaults to input + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new input field based on specified parameters.</returns>
        public string AddInputField(string fieldName, bool required, int maxLength = 0, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false)
        {
            RazorView<TLanguage> addInputFieldTemplate = new RazorView<TLanguage>(AddInputFieldTemplate, m_exceptionHandler);
            DynamicViewBag viewBag = addInputFieldTemplate.ViewBag;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"input{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("MaxLength", maxLength);
            viewBag.AddValue("InputType", inputType ?? "text");
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("RequiredDataBinding", requiredDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            return addInputFieldTemplate.Execute();
        }

        /// <summary>
        /// Generates template based text area field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="TModel">Modeled table.</typeparam>
        /// <param name="fieldName">Field name for text area field.</param>
        /// <param name="rows">Number of rows for text area.</param>
        /// <param name="fieldLabel">Label name for text area field, pulls from <see cref="LabelAttribute"/> if defined, otherwise defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for text area field; defaults to text + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new text area field based on modeled table field attributes.</returns>
        public string AddTextAreaField<TModel>(string fieldName, int rows = 2, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false) where TModel : class, new()
        {
            TableOperations<TModel> tableOperations = Table<TModel>();
            StringLengthAttribute stringLengthAttribute;
            RegularExpressionAttribute regularExpressionAttribute;

            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (tableOperations.TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            tableOperations.TryGetFieldAttribute(fieldName, out stringLengthAttribute);
            tableOperations.TryGetFieldAttribute(fieldName, out regularExpressionAttribute);

            if ((object)regularExpressionAttribute != null)
            {
                string observableReference;

                if (string.IsNullOrEmpty(groupDataBinding))
                    observableReference = $"viewModel.currentRecord().{fieldName}";
                else // "with: $root.connectionString"
                    observableReference = $"viewModel.{groupDataBinding.Substring(groupDataBinding.IndexOf('.') + 1)}";

                AddFieldValidation(observableReference, regularExpressionAttribute.Pattern, regularExpressionAttribute.ErrorMessage ?? "Invalid format.");
            }

            AddFieldValueInitializer<TModel>(fieldName);

            return AddTextAreaField(fieldName, tableOperations.FieldHasAttribute<RequiredAttribute>(fieldName),
                stringLengthAttribute?.MaximumLength ?? 0, rows, fieldLabel, fieldID, groupDataBinding, labelDataBinding, requiredDataBinding, customDataBinding, dependencyFieldName, toolTip, initialFocus);
        }

        /// <summary>
        /// Generates template based text area field based on specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name for text area field.</param>
        /// <param name="required">Determines if field name is required.</param>
        /// <param name="maxLength">Defines maximum text area field length.</param>
        /// <param name="rows">Number of rows for text area.</param>
        /// <param name="fieldLabel">Label name for text area field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for text area field; defaults to text + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new text area field based on specified parameters.</returns>
        public string AddTextAreaField(string fieldName, bool required, int maxLength = 0, int rows = 2, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false)
        {
            RazorView<TLanguage> addTextAreaTemplate = new RazorView<TLanguage>(AddTextAreaFieldTemplate, m_exceptionHandler);
            DynamicViewBag viewBag = addTextAreaTemplate.ViewBag;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"text{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("MaxLength", maxLength);
            viewBag.AddValue("Rows", rows);
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("RequiredDataBinding", requiredDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            return addTextAreaTemplate.Execute();
        }

        /// <summary>
        /// Generates template based select field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="TModel">Modeled table for select field.</typeparam>
        /// <typeparam name="TOption">Modeled table for option data.</typeparam>
        /// <param name="fieldName">Field name for value of select field.</param>
        /// <param name="optionValueFieldName">Field name for ID of option data.</param>
        /// <param name="optionLabelFieldName">Field name for label of option data, defaults to <paramref name="optionValueFieldName"/></param>
        /// <param name="optionSortFieldName">Field name for sort order of option data, defaults to <paramref name="optionLabelFieldName"/></param>
        /// <param name="fieldLabel">Label name for select field, pulls from <see cref="LabelAttribute"/> if defined, otherwise defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for select field; defaults to select + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="optionDataBinding">Data-bind operations to apply to each option value, if any.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>Generated HTML for new text field based on modeled table field attributes.</returns>
        public string AddSelectField<TModel, TOption>(string fieldName, string optionValueFieldName, string optionLabelFieldName = null, string optionSortFieldName = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string optionDataBinding = null, string toolTip = null, bool initialFocus = false, RecordRestriction restriction = null) where TModel : class, new() where TOption : class, new()
        {
            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (Table<TModel>().TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            AddFieldValueInitializer<TModel>(fieldName);

            return AddSelectField<TOption>(fieldName, Table<TModel>().FieldHasAttribute<RequiredAttribute>(fieldName),
                optionValueFieldName, optionLabelFieldName, optionSortFieldName, fieldLabel, fieldID, groupDataBinding, labelDataBinding, customDataBinding, dependencyFieldName, optionDataBinding, toolTip, initialFocus, restriction);
        }

        /// <summary>
        /// Generates template based select field based on specified parameters.
        /// </summary>
        /// <typeparam name="TOption">Modeled table for option data.</typeparam>
        /// <param name="fieldName">Field name for value of select field.</param>
        /// <param name="required">Determines if field name is required.</param>
        /// <param name="optionValueFieldName">Field name for ID of option data.</param>
        /// <param name="optionLabelFieldName">Field name for label of option data, defaults to <paramref name="optionValueFieldName"/></param>
        /// <param name="optionSortFieldName">Field name for sort order of option data, defaults to <paramref name="optionLabelFieldName"/></param>
        /// <param name="fieldLabel">Label name for select field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for select field; defaults to select + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="optionDataBinding">Data-bind operations to apply to each option value, if any.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>Generated HTML for new text field based on specified parameters.</returns>
        public string AddSelectField<TOption>(string fieldName, bool required, string optionValueFieldName, string optionLabelFieldName = null, string optionSortFieldName = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string optionDataBinding = null, string toolTip = null, bool initialFocus = false, RecordRestriction restriction = null) where TOption : class, new()
        {
            RazorView<TLanguage> addSelectFieldTemplate = new RazorView<TLanguage>(AddSelectFieldTemplate, m_exceptionHandler);
            DynamicViewBag viewBag = addSelectFieldTemplate.ViewBag;
            TableOperations<TOption> optionTableOperations = Table<TOption>();
            Dictionary<string, string> options = new Dictionary<string, string>();
            string optionTableName = typeof(TOption).Name;

            optionLabelFieldName = optionLabelFieldName ?? optionValueFieldName;
            optionSortFieldName = optionSortFieldName ?? optionLabelFieldName;
            fieldLabel = fieldLabel ?? optionTableName;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"select{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("FieldLabel", fieldLabel);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);
            viewBag.AddValue("OptionDataBinding", optionDataBinding);

            foreach (TOption record in Table<TOption>().QueryRecords(optionSortFieldName, restriction))
            {
                if (record != null)
                    options.Add(optionTableOperations.GetFieldValue(record, optionValueFieldName).ToString(), optionTableOperations.GetFieldValue(record, optionLabelFieldName).ToNonNullString(fieldLabel));
            }

            viewBag.AddValue("Options", options);

            return addSelectFieldTemplate.Execute();
        }

        /// <summary>
        /// Generates template based check box field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="TModel">Modeled table.</typeparam>
        /// <param name="fieldName">Field name for value of check box field.</param>
        /// <param name="fieldLabel">Label name for check box field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for check box field; defaults to check + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new check box field based on modeled table field attributes.</returns>
        public string AddCheckBoxField<TModel>(string fieldName, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false) where TModel : class, new()
        {
            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (Table<TModel>().TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            AddFieldValueInitializer<TModel>(fieldName);

            return AddCheckBoxField(fieldName, fieldLabel, fieldID, groupDataBinding, labelDataBinding, customDataBinding, dependencyFieldName, toolTip, initialFocus);
        }

        /// <summary>
        /// Generates template based check box field based on specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name for value of check box field.</param>
        /// <param name="fieldLabel">Label name for check box field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for check box field; defaults to check + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new check box field based on specified parameters.</returns>
        public string AddCheckBoxField(string fieldName, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false)
        {
            RazorView<TLanguage> addCheckBoxFieldTemplate = new RazorView<TLanguage>(AddCheckBoxFieldTemplate, m_exceptionHandler);
            DynamicViewBag viewBag = addCheckBoxFieldTemplate.ViewBag;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"check{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            return addCheckBoxFieldTemplate.Execute();
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConcurrentDictionary<Type, RecordOperationsCache> s_recordOperationCaches;

        // Static Constructor
        static DataContext()
        {
            s_recordOperationCaches = new ConcurrentDictionary<Type, RecordOperationsCache>();
        }

        // Static Methods
        private static bool IsNumericType(Type type)
        {
            return
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(Int24) ||
                type == typeof(UInt24) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal);
        }

        #endregion
    }
}
