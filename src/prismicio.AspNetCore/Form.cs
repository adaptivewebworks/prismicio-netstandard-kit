﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace prismic
{

    public class Form
    {

        public class Field
        {
            public string Type { get; }
            public bool IsMultiple { get; }
            public string DefaultValue { get; }

            public Field(string type, bool multipl, string defaultValue)
            {
                Type = type;
                IsMultiple = multipl;
                DefaultValue = defaultValue;
            }

            public static Field Parse(JToken json)
            {
                string type = (string)json["type"];
                string defaultValue = (json["default"] != null ? (string)json["default"] : null);
                bool multiple = (json["multiple"] != null ? (Boolean)json["multiple"] : false);
                return new Field(type, multiple, defaultValue);
            }

        }

        public string Name { get; }
        public string Method { get; }
        public string Rel { get; }
        public string Enctype { get; }
        public string Action { get; }
        public IDictionary<string, Field> Fields { get; }

        public Form(string name, string method, string rel, string enctype, string action, IDictionary<string, Field> fields)
        {
            Name = name;
            Method = method.ToUpper();
            Rel = rel;
            Enctype = enctype;
            Action = action;
            Fields = new Dictionary<string, Field>(fields);
        }

        public override string ToString() => Method + " " + Action;

        public static Form Parse(JObject json)
        {
            String name = (string)json["name"];
            String method = (string)json["method"];
            String rel = (string)json["rel"];
            String enctype = (string)json["enctype"];
            String action = (string)json["action"];

            //TODO... seen this pattern in API Data refactor me...
            var fields = new Dictionary<string, Field>();
            foreach (KeyValuePair<string, JToken> t in ((JObject)json["fields"]))
            {
                fields[t.Key] = Field.Parse((JObject)t.Value);
            }

            return new Form(name, method, rel, enctype, action, fields);
        }

        /**
         * The object you will use to perform queries. At the moment, only queries of the type "SearchForm" exist in prismic.io's APIs.
         * There is one named "everything", that allow to query through the while repository, and there is also one per collection
         * created by prismic.io administrators in the writing-room.
         *
         * From an {@link Api} object, you get a SearchForm form like this: <code>api.getForm("everything");</code>
         *
         * Then, from a SearchForm form, you query like this: <code>search.query("[[:d = at(document.type, "Product")]]").ref(ref).submit();</code>
         */
        public class SearchForm
        {

            private readonly Api api;
            private readonly Form form;
            private readonly IDictionary<string, StringValues> data;

            public SearchForm(Api api, Form form)
            {
                this.api = api;
                this.form = form;

                //TODO... seen this pattern in API Data refactor me...
                data = new Dictionary<String, StringValues>();
                foreach (KeyValuePair<String, Field> entry in form.Fields)
                {
                    if (entry.Value.DefaultValue != null)
                    {
                        data[entry.Key] = new StringValues(entry.Value.DefaultValue);
                    }
                }
            }

            /**
             * Allows to set one of the form's fields, such as "q" for the query field, or the "ordering" field, or the "pageSize" field.
             * The field must exist in the RESTful description that is in the /api document. To be on the safe side, you should use the
             * specialized methods, and use <code>searchForm.orderings(o)</code> rather than <code>searchForm.set("orderings", o)</code>
             * if they exist.
             *
             * @param field the name of the field to set
             * @param value the value with which to set it
             * @return the current form, in order to chain those calls
             */
            public SearchForm Set(String field, String value)
            {
                if (value == null)
                {
                    // null value, do nothing
                    return this;
                }
                Field fieldDesc = form.Fields[field];
                if (fieldDesc == null)
                {
                    throw new ArgumentException("Unknown field " + field);
                }
                if (fieldDesc.IsMultiple)
                {
                    IList<String> existingValue;
                    if (data.ContainsKey(field))
                    {
                        existingValue = data[field];
                    }
                    else
                    {
                        existingValue = new List<String>();
                    }
                    existingValue.Add(value);
                    data[field] = new StringValues(existingValue.ToArray());
                }
                else
                {
                    data[field] = new StringValues(value);
                }
                return this;
            }

            /**
             * A simple helper to set numeric value; see set(String,String).
             * @param field the name of the field to set
             * @param value target value
             * @return the current form, in order to chain those calls
             */
            public SearchForm Set(String field, int value)
            {
                Field fieldDesc = form.Fields[field];
                if (fieldDesc == null)
                {
                    throw new ArgumentException("Unknown field " + field);
                }
                if ("Integer" != fieldDesc.Type)
                {
                    throw new ArgumentException("Cannot set an Integer value to field " + field + " of type " + fieldDesc.Type);
                }
                return Set(field, value.ToString());
            }

            /**
             * Allows to set the ref on which you wish to be performing the query.
             *
             * This is mandatory to submit a query; if you call <code>api.getForm("everything").submit();</code>, the kit will complain!
             *
             * Please do not forge the ref dynamically in this call, like this: <code>ref(api.getMaster())</code>.
             * Prefer to set a ref variable once for your whole webpage, and use that variable in this method: <code>ref(ref)</code>.
             * That way, you can change this variable's assignment once, and trivially set your whole webpage into the future or the past.
             *
             * @param ref the ref object representing the ref on which you wish to query
             * @return the current form, in order to chain those calls
             */
            public SearchForm Ref(Ref myref) => Ref(myref.Reference);

            /**
             * Allows to set the ref on which you wish to be performing the query.
             *
             * This is mandatory to submit a query; if you call <code>api.getForm("everything").submit();</code>, the kit will complain!
             *
             * Please do not forge the ref dynamically in this call, like this: <code>ref(api.getMaster().getRef())</code>.
             * Prefer to set a ref variable once for your whole webpage, and use that variable in this method: <code>ref(ref)</code>.
             * That way, you can change this variable's assignment once, and trivially set your whole webpage into the future or the past.
             *
             * @param ref the ID of the ref on which you wish to query
             * @return the current form, in order to chain those calls
             */
            public SearchForm Ref(String myref) => Set("ref", myref);

            /**
             * Allows to set the size of the pagination of the query's response.
             *
             * The default value is 20; a call with a different page size will look like:
             * <code>api.getForm("everything").pageSize("15").ref(ref).submit();</code>.
             *
             * @param pageSize the size of the pagination you wish
             * @return the current form, in order to chain those calls
             */
            public SearchForm PageSize(String pageSize) => Set("pageSize", pageSize);

            /**
             * Allows to set the size of the pagination of the query's response.
             *
             * The default value is 20; a call with a different page size will look like:
             * <code>api.getForm("everything").pageSize(15).ref(ref).submit();</code>.
             *
             * @param pageSize the size of the pagination you wish
             * @return the current form, in order to chain those calls
             */
            public SearchForm PageSize(int pageSize) => Set("pageSize", pageSize);

            /**
            * Allows to set which page you want to get for your query.
            *
            * The default value is 1; a call for a different page will look like:
            * <code>api.getForm("everything").page("2").ref(ref).submit();</code>
            * (do remember that the default size of a page is 20, you can change it with <code>pageSize</code>)
            *
            * @param page the page number
            * @return the current form, in order to chain those calls
            */
            public SearchForm Page(String page) => Set("page", page);

            /**
             * Allows to set which page you want to get for your query.
             *
             * The default value is 1; a call for a different page will look like:
             * <code>api.getForm("everything").page(2).ref(ref).submit();</code>
             * (do remember that the default size of a page is 20, you can change it with <code>pageSize</code>)
             *
             * @param page the page number
             * @return the current form, in order to chain those calls
             */
            public SearchForm Page(int page) => Set("page", page);

            /**
             * Allows to set which ordering you want for your query.
             *
             * A call will look like:
             * <code>api.getForm("products").orderings("[my.product.price]").ref(ref).submit();</code>
             * Read prismic.io's API documentation to learn more about how to write orderings.
             *
             * @param orderings the orderings
             * @return the current form, in order to chain those calls
             */
            public SearchForm Orderings(String orderings) => Set("orderings", orderings);

            /**
             * Start the results after the id passed in parameter. Useful to get the documment following
             * a reference document for example.
             *
             * @param orderings the orderings
             * @return the current form, in order to chain those calls
             */
            public SearchForm Start(String id) => Set("start", id);

            /**
             * Restrict the document fragments to the set of fields specified.
             *
             * @param fields the fields to return
             * @return the current form, in order to chain those calls
             */
            public SearchForm Fetch(params String[] fields)
            {
                if (fields.Length == 0)
                {
                    return this; // Noop
                }
                else
                {
                    return Set("fetch", String.Join(",", fields));
                }
            }

            /**
             * Include the specified fragment in the details of DocumentLink
             *
             * @param fields the fields to return
             * @return the current form, in order to chain those calls
             */
            public SearchForm FetchLinks(params String[] fields)
            {
                if (fields.Length == 0)
                {
                    return this; // Noop
                }
                else
                {
                    return Set("fetchLinks", String.Join(",", fields));
                }
            }

            // Temporary hack for Backward compatibility
            private string Strip(String q)
            {
                if (q == null) 
                    return "";
                
                var tq = q.Trim();
                if (tq.IndexOf("[") == 0 && tq.LastIndexOf("]") == tq.Length - 1)
                {
                    return tq.Substring(1, tq.Length - 1);
                }
                return tq;
            }

            /**
             * Allows to set the query field of the current form. For instance:
             * <code>search.query("[[:d = at(document.type, "Product")]]");</code>
             * Look up prismic.io's documentation online to discover the possible query predicates.
             *
             * Beware: a query is a list of predicates, therefore, it always starts with "[[" and ends with "]]".
             *
             * @param q the query to pass
             * @return the current form, in order to chain those calls
             */
            public SearchForm Query(String q)
            {
                Field fieldDesc = form.Fields["q"];
                if (fieldDesc != null && fieldDesc.IsMultiple)
                {
                    return Set("q", q);
                }
                else
                {
                    var value = new StringValues("[ " + (form.Fields.ContainsKey("q") ? Strip(form.Fields["q"].DefaultValue) : "") + " " + Strip(q) + " ]");
                    data.Add("q", value);
                    return this;
                }
            }

            /**
             * Allows to set the query field of the current form, using Predicate objects. Example:
             * <code>search.query(Predicates.at("document.type", "Product"));</code>
             * See io.prismic.Predicates for more helper methods.
             *
             * @param predicates any number of predicate, is more than one is provided documents that satisfy all predicates will be returned ("AND" query)
             * @return the current form, in order to chain those calls
             */
            public SearchForm Query(params IPredicate[] predicates)
            {
                String result = "";
                foreach (Predicate p in predicates)
                {
                    result += p.q();
                }
                return this.Query("[" + result + "]");
            }

            /**
             * The method to call to perform and retrieve your query.
             *
             * Please make sure you're set a ref on this SearchForm form before querying, or the kit will complain!
             *
             * @return the list of documents, that can be directly used as such.
             */
            public Task<Response> Submit()
            {
                if ("GET" == form.Method && "application/x-www-form-urlencoded" == form.Enctype)
                {
                    var url = form.Action;
                    var sep = form.Action.Contains("?") ? "&" : "?";
                    foreach (KeyValuePair<String, StringValues> d in data)
                    {
                        foreach (String v in d.Value)
                        {
                            url += sep;
                            url += d.Key;
                            url += "=";
                            url += WebUtility.UrlEncode(v);
                            sep = "&";
                        }
                    }
                    // var uri = new Uri(form.Action);
                    return api.Fetch(url);
                }
                else
                {
                    throw new Error(Error.ErrorCode.UNEXPECTED, "Form type not supported");
                }
            }

            public override string ToString() => DictionaryExtensions.GetQueryString(this.data);

            public string toString() => this.ToString(); // Backwards compatability...

        }

    }

    internal static class DictionaryExtensions
    {
        internal static string GetQueryString(IDictionary<string, StringValues> values)
        {
            var qb = new QueryBuilder();

            foreach (var value in values)
				qb.Add(value.Key, value.Value.ToString());

            return qb.ToString();
        }
    }

}

