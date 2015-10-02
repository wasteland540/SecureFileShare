using System.IO;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;

namespace SecureFileShare.App.ViewModels.Help
{
    public class HelpViewModel : ViewModelBase
    {
        private IDocumentPaginatorSource _document;

        public IDocumentPaginatorSource Document
        {
            get
            {
                var xps = new XpsDocument(@"helpInstrucSFS.xps", FileAccess.Read);
                _document = _document ?? xps.GetFixedDocumentSequence();

                return _document;
            }
        }
    }
}