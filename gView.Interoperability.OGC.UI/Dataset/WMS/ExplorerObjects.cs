using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.system.UI;
using gView.Framework.IO;
using gView.Interoperability.OGC.Dataset.WMS;
using gView.OGC.UI;
using gView.Framework.Globalisation;
using gView.Framework.OGC.UI;

namespace gView.Interoperability.OGC.UI.Dataset.WMS
{
    [gView.Framework.system.RegisterPlugIn("41C75FD2-9AD0-4457-8248-E55EDA0C114E")]
    public class WMSExplorerObject : ExplorerParentObject, IOgcGroupExplorerObject
    {
        IExplorerIcon _icon = new WMSIcon();

        public WMSExplorerObject() : base(null, null) { }
        public WMSExplorerObject(IExplorerObject parent)
            : base(parent, null)
        {
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return "Web Services"; }
        }

        public string FullName
        {
            get { return "OGC/Web"; }
        }

        public string Type
        {
            get { return "OGC.WMS Connections"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public new object Object
        {
            get { return null; }
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }

        #endregion

        #region IExplorerParentObject Member

        public override void Refresh()
        {
            base.Refresh();
            base.AddChildObject(new WMSNewConnectionExplorerObject(this));

            ConfigTextStream stream = new ConfigTextStream("ogc_connections");
            string connStr, id;
            while ((connStr = stream.Read(out id)) != null)
            {
                base.AddChildObject(new WMSServiceExplorerObject(this, id, connStr));
            }
            stream.Close();
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            if (FullName == this.FullName)
            {
                WMSExplorerObject exObject = new WMSExplorerObject(this.ParentExplorerObject);
                cache.Append(exObject);
                return exObject;
            }
            return null;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("4C8FC31A-D988-4D6A-94C5-849237FB8E70")]
    public class WMSNewConnectionExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        IExplorerIcon _icon = new WMSNewConnectionIcon();
        IExplorerParentObject _parent;

        public WMSNewConnectionExplorerObject()
            : base(null, null)
        {
            _parent = null;
        }
        public WMSNewConnectionExplorerObject(IExplorerParentObject parent)
            : base(parent as IExplorerObject, null)
        {
            _parent = parent;
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return LocalizedResources.GetResString("String.NewConnection", "New WMS Connection..."); }
        }

        public string FullName
        {
            get { return ""; }
        }

        public string Type
        {
            get { return ""; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public void Dispose()
        {

        }

        public new object Object
        {
            get { return null; }
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }

        #endregion

        #region IExplorerObjectDoubleClick Member

        public void ExplorerObjectDoubleClick(ExplorerObjectEventArgs e)
        {
            FormNewConnection dlg = new FormNewConnection();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string connStr = dlg.ConnectionString;
                if (connStr == String.Empty) return;

                string id = String.Empty;
                switch (ConfigTextStream.ExtractValue(connStr, "service").ToUpper())
                {
                    case "WMS_WFS":
                    case "WMS":
                        Uri wmsUri = new Uri(ConfigTextStream.ExtractValue(connStr, "wms"));
                        id = wmsUri.Host;
                        break;
                    case "WFS":
                        Uri wfsUri = new Uri(ConfigTextStream.ExtractValue(connStr, "wfs"));
                        id = wfsUri.Host;
                        break;
                    default:
                        return;
                }
                if (dlg.ServiceName != String.Empty)
                    id = dlg.ServiceName + "@" + id;

                ConfigTextStream stream = new ConfigTextStream("ogc_connections", true, true);
                stream.Write(connStr, ref id);
                stream.Close();

                e.NewExplorerObject = new WMSServiceExplorerObject(_parent, id, dlg.ConnectionString);
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];
            return null;
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return (parentExObject is WMSExplorerObject);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return e.NewExplorerObject;
        }

        #endregion
    }

    //[gView.Framework.system.RegisterPlugIn("90DCB8C8-4570-4851-B924-CCC359A1B69F")]
    public class WMSServiceExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable
    {
        private IExplorerIcon _icon = new WMSConnectionIcon();
        private string _name = "", _connectionString = "";
        private WMSClass _class = null;
        private IExplorerParentObject _parent;
        private string _type = "OGC WMS Service";

        internal WMSServiceExplorerObject(IExplorerParentObject parent, string name, string connectionString)
            : base(parent as IExplorerObject, typeof(WMSClass))
        {
            _name = name;
            _connectionString = connectionString;

            switch (ConfigTextStream.ExtractValue(_connectionString, "service").ToUpper())
            {
                case "WMS":
                    _type = "OGC WMS Service";
                    break;
                case "WFS":
                    _type = "OGC WFS Service";
                    _icon = new WFSConnectionIcon();
                    break;
                case "WMS_WFS":
                    _type = "OGC WMS/WFS Service";
                    _icon = new WMSWFSConnectionIcon();
                    break;
                default:
                    _type = "Unknown OGC Service Type!!";
                    break;
            }
            _parent = parent;
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return _name; }
        }

        public string FullName
        {
            get
            {
                if (!(_parent is IExplorerObject)) return "";
                return ((IExplorerObject)_parent).FullName + @"\" + _name;
            }
        }

        public string Type
        {
            get { return _type; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public void Dispose()
        {

        }

        public new object Object
        {
            get
            {
                if (_class == null)
                {
                    WMSDataset dataset = new WMSDataset(_connectionString, _name);
                    //dataset.Open(); // kein open, weil sonst ein GET_SERVICE_INFO durchgeführt wird...
                    switch (ConfigTextStream.ExtractValue(_connectionString, "service").ToUpper())
                    {
                        case "WMS":
                        case "WMS_WFS":
                            if (dataset.Elements.Count == 0)
                            {
                                dataset.Dispose();
                                return null;
                            }
                            return _class = dataset.Elements[0].Class as WMSClass;
                        case "WFS":
                            return dataset;
                        default:
                            return null;
                    }
                }

                return _class;
            }
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1) return null;

            string cnName = FullName.Substring(0, lastIndex);
            string svName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            WMSExplorerObject cnObject = new WMSExplorerObject(this);
            cnObject = cnObject.CreateInstanceByFullName(cnName, cache) as WMSExplorerObject;
            if (cnObject == null || cnObject.ChildObjects == null) return null;

            foreach (IExplorerObject exObject in cnObject.ChildObjects)
            {
                if (exObject.Name == svName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted;

        public bool DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            ConfigTextStream stream = new ConfigTextStream("ogc_connections", true, true);
            stream.Remove(this.Name, _connectionString);
            stream.Close();
            if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
            return true;
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed;

        public bool RenameExplorerObject(string newName)
        {
            bool ret = false;
            ConfigTextStream stream = new ConfigTextStream("ogc_connections", true, true);
            ret = stream.ReplaceHoleLine(ConfigTextStream.BuildLine(this.Name, _connectionString), ConfigTextStream.BuildLine(newName, _connectionString));
            stream.Close();

            if (ret == true)
            {
                _name = newName;
                if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            }
            return ret;
        }

        #endregion
    }

    internal class WMSIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("20E55398-AF55-49ab-8363-D893669DEE1B"); }
        }

        public System.Drawing.Image Image
        {
            get { return (new Icons()).imageList1.Images[0]; }
        }

        #endregion
    }
    internal class WMSNewConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("840FED3E-E4A0-477b-97C5-261024A8992D"); }
        }

        public System.Drawing.Image Image
        {
            get { return (new Icons()).imageList1.Images[1]; }
        }

        #endregion
    }
    internal class WMSConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("F596A0B8-9C5D-48ca-B1B1-F9B2A8F2E19B"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Interoperability.OGC.UI.Properties.Resources.wms; }
        }

        #endregion
    }

    internal class WFSConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("5B141ECA-6E0A-4d48-8A2F-8B679FA6F22E"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Interoperability.OGC.UI.Properties.Resources.wfs; }
        }

        #endregion
    }

    internal class WMSWFSConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("B83C63CE-70EC-4890-9277-A352127786A4"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Interoperability.OGC.UI.Properties.Resources.wms_wfs; }
        }

        #endregion
    }
}
