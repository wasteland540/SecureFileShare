using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using Microsoft.Practices.Unity;
using SecureFileShare.App.Commands;
using SecureFileShare.App.Messages;
using SecureFileShare.App.Views.Contacts;
using SecureFileShare.DataAccessLayer;
using SecureFileShare.Model;

namespace SecureFileShare.App.ViewModels.Contacts
{
    public class ContactsViewModel : ViewModelBase
    {
        private readonly IDataAccessLayer _database;
        private readonly ILog _logger = LogManager.GetLogger(typeof (ContactsViewModel));
        private readonly IMessenger _messenger;

        private ICommand _addCommand;
        private AddEditContactView _addEditContactView;
        private ICommand _contactDoubleClickCommand;

        private List<Contact> _contacts;
        private ICommand _deleteCommand;
        private ICommand _editCommand;
        private string _searchQuery;
        private Contact _selectedContact;

        public ContactsViewModel(IDataAccessLayer database, IMessenger messenger)
        {
            _database = database;
            _messenger = messenger;

            _messenger.Register<AddEditContactViewClosedMsg>(this, OnAddEditContactViewClosedMsg);
            _messenger.Register<SaveContactMsg>(this, OnSaveContactMsg);
            _messenger.Register<DeleteContactConfirmMsg>(this, OnDeleteContactConfirmMsg);

            Contacts = _database.GetAll<Contact>();
        }

        public bool IsSelectionMode { get; set; }

        public void OnClosing()
        {
            _messenger.Unregister<AddEditContactViewClosedMsg>(this, OnAddEditContactViewClosedMsg);
            _messenger.Unregister<SaveContactMsg>(this, OnSaveContactMsg);
            _messenger.Unregister<DeleteContactConfirmMsg>(this, OnDeleteContactConfirmMsg);
        }

        #region Properties

        public string SearchQuery
        {
            get { return _searchQuery; }
            set
            {
                if (value != null && value != _searchQuery)
                {
                    _searchQuery = value;
                    _logger.Info("update contacts by search query: " + _searchQuery);
                    Contacts =
                        _database.GetAll<Contact>()
                            .Where(c => c.Name.ToLower().Contains(_searchQuery.ToLower()))
                            .ToList();

                    RaisePropertyChanged("SearchQuery");
                }
            }
        }

        public ICommand AddCommand
        {
            get
            {
                _addCommand = _addCommand ?? new DelegateCommand(Add);
                return _addCommand;
            }
        }

        public ICommand EditCommand
        {
            get
            {
                _editCommand = _editCommand ?? new DelegateCommand(Edit);
                return _editCommand;
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                _deleteCommand = _deleteCommand ?? new DelegateCommand(Delete);
                return _deleteCommand;
            }
        }

        public ICommand ContactDoubleClickCommand
        {
            get
            {
                _contactDoubleClickCommand = _contactDoubleClickCommand ?? new DelegateCommand(ContactDoubleClick);
                return _contactDoubleClickCommand;
            }
        }

        public List<Contact> Contacts
        {
            get { return _contacts; }
            set
            {
                if (value != null && value != _contacts)
                {
                    _contacts = value;
                    RaisePropertyChanged("Contacts");
                }
            }
        }

        public Contact SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                if (value != null && value != _selectedContact)
                {
                    _selectedContact = value;
                    RaisePropertyChanged("SelectedContact");
                }
            }
        }

        #endregion Properties

        #region Private Methods

        private void Add(object obj)
        {
            if (_addEditContactView == null)
            {
                _logger.Info("show add/edit contact view");
                _addEditContactView = Container.Resolve<AddEditContactView>();
                _addEditContactView.Show();

                var viewmodel = (AddEditContactViewModel) _addEditContactView.DataContext;
                viewmodel.IsNew = true;
            }
            else
            {
                _logger.Warn("add/edit contact view already open");
                _logger.Info("push view in foreground");

                var viewmodel = (AddEditContactViewModel) _addEditContactView.DataContext;
                viewmodel.IsNew = true;

                _addEditContactView.Focus();
            }
        }

        private void Edit(object obj)
        {
            if (_addEditContactView == null)
            {
                _logger.Info("show add/edit contact view");
                _addEditContactView = Container.Resolve<AddEditContactView>();
                _addEditContactView.Show();

                var viewmodel = (AddEditContactViewModel) _addEditContactView.DataContext;
                viewmodel.OriginalName = _selectedContact.Name;
                viewmodel.Name = _selectedContact.Name;
                viewmodel.PublicKey = _selectedContact.PublicKey;
                viewmodel.KeyFilePath = "No new key file selected, but there is still a key for this contact!";
                viewmodel.IsNew = false;
            }
            else
            {
                _logger.Warn("add/edit contact view already open");
                _logger.Info("push view in foreground");

                var viewmodel = (AddEditContactViewModel) _addEditContactView.DataContext;
                viewmodel.OriginalName = _selectedContact.Name;
                viewmodel.Name = _selectedContact.Name;
                viewmodel.PublicKey = _selectedContact.PublicKey;
                viewmodel.KeyFilePath = "No new key file selected, but there is still a key for this contact!";
                viewmodel.IsNew = false;

                _addEditContactView.Focus();
            }
        }

        private void Delete(object obj)
        {
            //TODO: implment for context menu!

            _messenger.Send(new DeleteContactRequestMsg());
        }

        private void ContactDoubleClick(object obj)
        {
            if (IsSelectionMode)
            {
                throw new NotImplementedException();
            }
            Edit(null);
        }

        private void OnAddEditContactViewClosedMsg(AddEditContactViewClosedMsg msg)
        {
            _logger.Info("set add/edit contact view to null");
            _addEditContactView = null;
        }

        private void OnSaveContactMsg(SaveContactMsg msg)
        {
            _logger.Info("reload contacts");
            ReloadContacts();
        }

        private void ReloadContacts()
        {
            Contacts = _database.GetAll<Contact>();
        }

        private void OnDeleteContactConfirmMsg(DeleteContactConfirmMsg obj)
        {
            if (_selectedContact != null)
            {
                _logger.Info("delete selected contact");
                _database.Delete(_selectedContact);
                ReloadContacts();
            }
        }

        #endregion Private Methods
    }
}