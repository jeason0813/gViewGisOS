using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using gView.Framework.UI;
using gView.Framework.system;
using gView.Framework.IO;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system.UI;
using gView.Framework.FDB;
using gView.OGC;
using gView.Framework.Db.UI;
using gView.Framework.OGC.UI;
using gView.Framework.Globalisation;
using System.Windows.Forms;

namespace gView.DataSources.MSSqlSpatial.UI
{
    [gView.Framework.system.RegisterPlugIn("25F9A2DE-D54F-4DCD-832A-C2EFB544CF8F")]
    public class MsSql2008SpatialExplorerGroupObject : ExplorerParentObject, IOgcGroupExplorerObject
    {
        private IExplorerIcon _icon = new MsSql2008SpatialIcon();

        public MsSql2008SpatialExplorerGroupObject()
            : base(null, null)
        { }

        #region IExplorerGroupObject Members

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get { return "MsSql 2008 Spatial Geometry"; }
        }

        public string FullName
        {
            get { return @"OGC\MsSql2008Spatial"; }
        }

        public string Type
        {
            get { return "MsSql Spatial Connection"; }
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

        #region IExplorerParentObject Members

        public override void Refresh()
        {
            base.Refresh();
            base.AddChildObject(new MsSql2008SpatialNewConnectionObject(this));

            ConfigTextStream stream = new ConfigTextStream("MsSql2008Spatial_connections");
            string connStr, id;
            while ((connStr = stream.Read(out id)) != null)
            {
                base.AddChildObject(new MsSql2008SpatialExplorerObject(this, id, connStr));
            }
            stream.Close();
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            if (this.FullName == FullName)
            {
                MsSql2008SpatialExplorerGroupObject exObject = new MsSql2008SpatialExplorerGroupObject();
                cache.Append(exObject);
                return exObject;
            }

            return null;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("1039B5BC-9460-40FC-8837-9A87EDFBBA8E")]
    public class MsSql2008SpatialNewConnectionObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new MsSql2008SpatialNewConnectionIcon();

        public MsSql2008SpatialNewConnectionObject()
            : base(null, null)
        {
        }

        public MsSql2008SpatialNewConnectionObject(IExplorerObject parent)
            : base(parent, null)
        {
        }

        #region IExplorerSimpleObject Members

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get { return LocalizedResources.GetResString("String.NewConnection", "New Connection..."); }
        }

        public string FullName
        {
            get { return ""; }
        }

        public string Type
        {
            get { return "New MsSql 2008 Spatial Connection"; }
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

        #region IExplorerObjectDoubleClick Members

        public void ExplorerObjectDoubleClick(ExplorerObjectEventArgs e)
        {
            FormConnectionString dlg = new FormConnectionString();
            dlg.ProviderID = "mssql";
            dlg.UseProviderInConnectionString = false;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string connStr = dlg.ConnectionString;
                ConfigTextStream stream = new ConfigTextStream("MsSql2008Spatial_connections", true, true);
                string id = ConfigTextStream.ExtractValue(connStr, "Database");
                id += "@" + ConfigTextStream.ExtractValue(connStr, "Server");
                if (id == "@") id = "MsSql 2008 Spatial Connection";
                stream.Write(connStr, ref id);
                stream.Close();

                e.NewExplorerObject = new MsSql2008SpatialExplorerObject(this.ParentExplorerObject, id, dlg.ConnectionString);
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
            return (parentExObject is MsSql2008SpatialExplorerGroupObject);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return e.NewExplorerObject;
        }

        #endregion
    }

    public class MsSql2008SpatialExplorerObject : gView.Framework.OGC.UI.ExplorerObjectFeatureClassImport, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable, ISerializableExplorerObject
    {
        private MsSql2008SpatialIcon _icon = new MsSql2008SpatialIcon();
        private string _server = "", _connectionString = "", _errMsg = "";
        private IFeatureDataset _dataset;

        public MsSql2008SpatialExplorerObject() : base(null,typeof(IFeatureDataset)) { }
        public MsSql2008SpatialExplorerObject(IExplorerObject parent, string server, string connectionString)
            : base(parent,typeof(IFeatureDataset))
        {
            _server = server;
            _connectionString = connectionString;
        }

        internal string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        #region IExplorerObject Members

        public string Name
        {
            get
            {
                return _server;
            }
        }

        public string FullName
        {
            get
            {
                return @"OGC\MsSql2008Spatial\" + _server;
            }
        }

        public string Type
        {
            get { return "MsSql2008Spatial Database"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        public object Object
        {
            get
            {
                if (_dataset != null) _dataset.Dispose();

                _dataset = new GeometryDataset();
                _dataset.ConnectionString = _connectionString;
                _dataset.Open();
                return _dataset;
            }
        }

        #endregion

        #region IExplorerParentObject Members

        public override void Refresh()
        {
            base.Refresh();
            GeometryDataset dataset = new GeometryDataset();
            dataset.ConnectionString = _connectionString;
            dataset.Open();

            List<IDatasetElement> elements = dataset.Elements;

            if (elements == null) return;
            foreach (IDatasetElement element in elements)
            {
                if (element.Class is IFeatureClass)
                {
                    base.AddChildObject(new MsSql2008SpatialFeatureClassExplorerObject(this, element));
                }
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            MsSql2008SpatialExplorerGroupObject group = new MsSql2008SpatialExplorerGroupObject();
            if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2) return null;

            group = (MsSql2008SpatialExplorerGroupObject)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

            foreach (IExplorerObject exObject in group.ChildObjects)
            {
                if (exObject.FullName == FullName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public bool DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            ConfigTextStream stream = new ConfigTextStream("MsSql2008Spatial_connections", true, true);
            stream.Remove(this.Name, _connectionString);
            stream.Close();
            if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
            return true;
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed = null;

        public bool RenameExplorerObject(string newName)
        {
            bool ret = false;
            ConfigTextStream stream = new ConfigTextStream("MsSql2008Spatial_connections", true, true);
            ret = stream.ReplaceHoleLine(ConfigTextStream.BuildLine(_server, _connectionString), ConfigTextStream.BuildLine(newName, _connectionString));
            stream.Close();

            if (ret == true)
            {
                _server = newName;
                if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            }
            return ret;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("BAC5EF61-8E60-48D5-9744-4260BDCDBD56")]
    public class MsSql2008SpatialFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject, IExplorerObjectDeletable
    {
        private string _fcname = "", _type = "";
        private IExplorerIcon _icon = null;
        private IFeatureClass _fc = null;
        private MsSql2008SpatialExplorerObject _parent = null;

        public MsSql2008SpatialFeatureClassExplorerObject() : base(null,typeof(IFeatureClass)) { }
        public MsSql2008SpatialFeatureClassExplorerObject(MsSql2008SpatialExplorerObject parent, IDatasetElement element)
            : base(parent,typeof(IFeatureClass))
        {
            if (element == null || !(element.Class is IFeatureClass)) return;

            _parent = parent;
            _fcname = element.Title;

            if (element.Class is IFeatureClass)
            {
                _fc = (IFeatureClass)element.Class;
                switch (_fc.GeometryType)
                {
                    case geometryType.Envelope:
                    case geometryType.Polygon:
                        _icon = new MsSql2008SpatialPolygonIcon();
                        _type = "Polygon Featureclass";
                        break;
                    case geometryType.Multipoint:
                    case geometryType.Point:
                        _icon = new MsSql2008SpatialPointIcon();
                        _type = "Point Featureclass";
                        break;
                    case geometryType.Polyline:
                        _icon = new MsSql2008SpatialLineIcon();
                        _type = "Polyline Featureclass";
                        break;
                    default:
                        _icon = new MsSql2008SpatialLineIcon();
                        _type = "Featureclass";
                        break;
                }
            }
        }

        internal string ConnectionString
        {
            get
            {
                if (_parent == null) return "";
                return _parent.ConnectionString;
            }
        }

        #region IExplorerObject Members

        public string Name
        {
            get { return _fcname; }
        }

        public string FullName
        {
            get
            {
                if (_parent == null) return "";
                return _parent.FullName + @"\" + this.Name;
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
            if (_fc != null)
            {
                _fc = null;
            }
        }
        public object Object
        {
            get
            {
                return _fc;
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1) return null;

            string dsName = FullName.Substring(0, lastIndex);
            string fcName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            MsSql2008SpatialExplorerObject dsObject = new MsSql2008SpatialExplorerObject();
            dsObject = dsObject.CreateInstanceByFullName(dsName, cache) as MsSql2008SpatialExplorerObject;
            if (dsObject == null || dsObject.ChildObjects == null) return null;

            foreach (IExplorerObject exObject in dsObject.ChildObjects)
            {
                if (exObject.Name == fcName)
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
            if (_parent.Object is IFeatureDatabase)
            {
                if (((IFeatureDatabase)_parent.Object).DeleteFeatureClass(this.Name))
                {
                    if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                    return true;
                }
                else
                {
                    MessageBox.Show("ERROR: " + ((IFeatureDatabase)_parent.Object).lastErrorMsg);
                    return false;
                }
            }
            return false;
        }

        #endregion
    }

    class MsSql2008SpatialIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("B22782D4-59D2-448a-A531-DE29B0067DE6");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.DataSources.MSSqlSpatial.UI.Properties.Resources.cat6;
            }
        }

        #endregion
    }

    class MsSql2008SpatialNewConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("56943EE9-DCE8-4ad9-9F13-D306A8A9210E");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.DataSources.MSSqlSpatial.UI.Properties.Resources.gps_point;
            }
        }

        #endregion
    }

    public class MsSql2008SpatialPointIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("372EBA30-1F19-4109-B476-8B158CAA6360"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.MSSqlSpatial.UI.Properties.Resources.img_32; }
        }

        #endregion
    }

    public class MsSql2008SpatialLineIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("5EA3477F-7616-4775-B233-72C94BE055CA"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.MSSqlSpatial.UI.Properties.Resources.img_33; }
        }

        #endregion
    }

    public class MsSql2008SpatialPolygonIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("D1297B06-4306-4d5d-BBC6-10E26792CE5F"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.MSSqlSpatial.UI.Properties.Resources.img_34; }
        }

        #endregion
    }
}
