using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Windows;
using System.Windows.Input;

namespace SorcererSoftware {
   public class ExpandoDependencyObject : DynamicObject, IDictionary<string, object>, INotifyCollectionChanged, INotifyPropertyChanged {
      #region DynamicObject

      readonly IDictionary<string, object> _members = new Dictionary<string, object>();
      public override bool TrySetMember(SetMemberBinder binder, object value) {
         if (binder.Name.EndsWith("Executed")) SetExecuted(Chop(binder.Name, "Executed"), value);
         else if (binder.Name.EndsWith("CanExecute")) SetCanExecute(Chop(binder.Name, "CanExecute"), value);
         else if (binder.Name.EndsWith("Changed")) SetChangeHandler(Chop(binder.Name, "Changed"), value);
         this[binder.Name] = value;
         return true;
      }

      public override bool TryGetMember(GetMemberBinder binder, out object result) {
         result = _members.ContainsKey(binder.Name) ? _members[binder.Name] : null;
         return result != null;
      }

      static string Chop(string name, string token) {
         return name.Substring(0, name.Length - token.Length);
      }

      #endregion

      #region Execution Handlers

      /// <summary>
      /// Fill this dictionary with Actions sent in with signatures like "CommadNameExecuted", where each token ends with "Executed".
      /// </summary>
      readonly IDictionary<RoutedCommand, dynamic> _commandExecuteds = new Dictionary<RoutedCommand, dynamic>();

      /// <summary>
      /// Fill this dictionary with Actions sent in with signatures like "CommadNameCanExecute", where each token ends with "CanExecute".
      /// </summary>
      readonly IDictionary<RoutedCommand, dynamic> _commandCanExecutes = new Dictionary<RoutedCommand, dynamic>();

      /// <summary>
      /// Unwrap the sender and eventargs from the parameter.
      /// </summary>
      /// <param name="command"></param>
      /// <param name="args"></param>
      public void TryExecute(ExecutedRoutedEventArgs args) {
         var command = (RoutedUICommand)args.Command;
         if (!_commandExecuteds.ContainsKey(command)) return;
         var action = _commandExecuteds[command];
         ExceptionHandler.Try(() => {
            action(args.Source, (RoutedEventArgs)args.Parameter);
            args.Handled = true;
         }, "Error running " + command.Name + "Executed");
      }

      public void TryCanExecute(object sender, CanExecuteRoutedEventArgs args) {
         var command = (RoutedCommand)args.Command;
         if (!_commandCanExecutes.ContainsKey(command)) {
            args.CanExecute = true;
            return;
         }
         var action = _commandExecuteds[command];
         ExceptionHandler.Try(() => action(sender, args), "Error running " + command.Name + "CanExecute");
      }

      void SetExecuted(string name, object value) {
         _commandExecuteds[DynamicCommands.GetCommand(name)] = value;
      }

      void SetCanExecute(string name, object value) {
         _commandCanExecutes[DynamicCommands.GetCommand(name)] = value;
      }

      #endregion

      #region ChangeHandlers

      readonly IDictionary<string, dynamic> _changeHandlers = new Dictionary<string, dynamic>();

      public void TryHandleChange(string name) {
         if (!_changeHandlers.ContainsKey(name)) return;
         var action = _changeHandlers[name];
         ExceptionHandler.Try(() => action(), "Error running change handler " + name + "Changed");
      }

      void SetChangeHandler(string name, object value) {
         _changeHandlers[name] = value;
      }

      #endregion

      #region NotifyChanged Interfaces

      public event PropertyChangedEventHandler PropertyChanged;
      public event NotifyCollectionChangedEventHandler CollectionChanged;

      void Notify(string prop) {
         if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
         TryHandleChange(prop);
      }
      void Notify(NotifyCollectionChangedAction action) {
         if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }

      #endregion

      #region IDictionary

      public void Add(string key, object value) {
         _members.Add(key, value);
         Notify(NotifyCollectionChangedAction.Add);
      }
      public bool ContainsKey(string key) { return _members.ContainsKey(key); }
      public ICollection<string> Keys { get { return _members.Keys; } }
      public bool Remove(string key) {
         var result = _members.Remove(key);
         Notify(NotifyCollectionChangedAction.Remove);
         return result;
      }
      public bool TryGetValue(string key, out object value) { return _members.TryGetValue(key, out value); }
      public ICollection<object> Values { get { return _members.Values; } }
      public object this[string key] {
         get { return _members[key]; }
         set {
            bool isNew = !_members.ContainsKey(key);
            _members[key] = value;
            if (isNew) {
               Notify(NotifyCollectionChangedAction.Add);
            } else {
               Notify(NotifyCollectionChangedAction.Replace);
               Notify(key);
            }
         }
      }
      public void Add(KeyValuePair<string, object> item) { _members.Add(item); Notify(NotifyCollectionChangedAction.Add); }
      public void Clear() { _members.Clear(); Notify(NotifyCollectionChangedAction.Reset); }
      public bool Contains(KeyValuePair<string, object> item) { return _members.Contains(item); }
      public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) { _members.CopyTo(array, arrayIndex); }
      public int Count { get { return _members.Count; } }
      public bool IsReadOnly { get { return _members.IsReadOnly; } }
      public bool Remove(KeyValuePair<string, object> item) {
         var result = _members.Remove(item);
         Notify(NotifyCollectionChangedAction.Remove);
         return result;
      }
      public IEnumerator<KeyValuePair<string, object>> GetEnumerator() { return _members.GetEnumerator(); }
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return _members.GetEnumerator(); }

      #endregion
   }
}
