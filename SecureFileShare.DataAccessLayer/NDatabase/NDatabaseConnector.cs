using System.Collections.Generic;
using System.Linq;
using NDatabase;
using NDatabase.Api;
using NDatabase.Api.Query;
using SecureFileShare.Model;

namespace SecureFileShare.DataAccessLayer.NDatabase
{
    public class NDatabaseConnector : IDataAccessLayer
    {
        private const string DatabaseName = "secureFileShare.db";
        private readonly IOdb _odb;

        public NDatabaseConnector()
        {
            _odb = OdbFactory.Open(DatabaseName);
        }

        public void Insert<T>(T value) where T : class, IDbObject
        {
            _odb.Store(value);
        }

        public void Delete<T>(T value) where T : class, IDbObject
        {
            _odb.Delete(value);
        }

        public void Update<T>(T value) where T : class, IDbObject
        {
            _odb.Store(value);
        }

        public List<T> GetAll<T>() where T : class, IDbObject
        {
            var resultList = new List<T>();

            IQuery query = _odb.Query<T>();
            resultList.AddRange(query.Execute<T>());

            return resultList;
        }

        public T GetSingleByName<T>(string name) where T : class, IDbObject
        {
            var resultList = new List<T>();

            IQuery query = _odb.Query<T>();
            resultList.AddRange(query.Execute<T>().Where(record => record.Name == name));

            return resultList.FirstOrDefault();
        }

        public void Close()
        {
            _odb.Close();
        }
    }
}