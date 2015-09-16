public class ElasticObject : DynamicObject,    
    ICustomMemberProvider // For Linq Dump
{
    private IDictionary<string, object> _dict = new Dictionary<string, object>();
    private IDictionary<int, object> _1DArray = null;
    
    public ElasticObject(params object[] templates)
    {
        if (templates != null) 
            templates.ForEach((template, index) => ApplyTemplate(template, index));
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        result = this[binder.Name];
        return true;
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        if (!value.GetType().IsArray && value.GetType().Name.Contains("AnonymousType")) value = new ElasticObject(value);
        
        this[binder.Name] = value;
        return true;
    }

    public object this[string key] 
    { 
        get
        {
            if (!_dict.ContainsKey(key)) _dict.Add(key, new ElasticObject());
            return _dict[key];
        }
        set
        {
            if (_dict.ContainsKey(key))
                _dict[key] = value;
            else
                _dict.Add(key, value);
        }
    }

    public object this[int index]
    {
        get
        {
            if (index == 0) return this;
            if (this._1DArray == null) this._1DArray = new Dictionary<int, object>();
            
            if (!this._1DArray.ContainsKey(index)) this._1DArray.Add(index, new ElasticObject());
            return this._1DArray[index];
        }
        set
        {
            if (this._1DArray == null) this._1DArray = new Dictionary<int, object>();
            
            if (this._1DArray.ContainsKey(index))
                this._1DArray[index] = value;
            else
                this._1DArray.Add(index, value);
        }
    }

    #region For LinqPad only

    public void Dump()
    {
        if (this._1DArray == null)
            this.Dump(int.MaxValue);
        else
        {
            var ordered = this._1DArray.Keys.OrderBy(k => k).Select(k => this._1DArray[k]);
            Enumerable.Union(new[] { this }, ordered).ToArray().Dump(int.MaxValue);
        }
    }

    public IEnumerable<string> GetNames()
    {
        if (this._dict.Keys.Count == 0 && this._1DArray != null)
            return this._1DArray.Keys.Select(k => k.ToString());
        else
            return this._dict.Keys;
    }

    public IEnumerable<Type> GetTypes()
    {
        if (this._dict.Keys.Count == 0 && this._1DArray != null)
            return this._1DArray.Values.Select(k => k.GetType());
        else
            return this._dict.Values.Select(x => x.GetType());
    }

    public IEnumerable<object> GetValues()
    {
        if (this._dict.Keys.Count == 0 && this._1DArray != null)
            return this._1DArray.Values;
        else
            return this._dict.Values;
    }

    #endregion

    private void ApplyTemplate(object value, int index = 0)
    {
        dynamic t = this[index];
        foreach (var property in value.GetType().GetProperties()) t[property.Name] = property.GetValue(value);
    }
}