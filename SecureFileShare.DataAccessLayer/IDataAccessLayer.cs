using System.Collections.Generic;
using SecureFileShare.Model;

namespace SecureFileShare.DataAccessLayer
{
    public interface IDataAccessLayer
    {
        void Insert<T>(T value) where T : class, IDbObject;

        void Delete<T>(T value) where T : class, IDbObject;

        void Update<T>(T value) where T : class, IDbObject;

        List<T> GetAll<T>() where T : class, IDbObject;

        T GetSingleByName<T>(string name) where T : class, IDbObject;

        void Close();
    }
}