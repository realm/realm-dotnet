[
  {
    "Id": "???",
    "Severity": 3,
    "Message": "CollectionErrors.UnsupportetDictionaryKeyProp  is a Dictionary<int, string> but only string keys are currently supported by Realm.",
    "Location": {
      "Path": null,
      "StartLine": 30,
      "StartColumn": 9,
      "EndLine": 30,
      "EndColumn": 78
    }
  },
  {
    "Id": "???",
    "Severity": 3,
    "Message": "CollectionErrors.SetofEmbeddedObj is a Set<EmbeddedObject> which is not supported. Embedded objects are always unique which is why List<EmbeddedObject> already has Set semantics.",
    "Location": {
      "Path": null,
      "StartLine": 32,
      "StartColumn": 9,
      "EndLine": 32,
      "EndColumn": 59
    }
  },
  {
    "Id": "???",
    "Severity": 3,
    "Message": "CollectionErrors.CollectionWithSetter has a setter but its type is a List which only supports getters.",
    "Location": {
      "Path": null,
      "StartLine": 34,
      "StartColumn": 9,
      "EndLine": 34,
      "EndColumn": 61
    }
  },
  {
    "Id": "???",
    "Severity": 3,
    "Message": "CollectionErrors.CollectionOfRealmInteger is an List<RealmInteger> which is not supported.",
    "Location": {
      "Path": null,
      "StartLine": 36,
      "StartColumn": 9,
      "EndLine": 36,
      "EndColumn": 74
    }
  },
  {
    "Id": "???",
    "Severity": 3,
    "Message": "CollectionErrors.CollectionOfUnsupportedType is an List but its generic type is System.DateTime which is not supported by Realm.",
    "Location": {
      "Path": null,
      "StartLine": 38,
      "StartColumn": 9,
      "EndLine": 38,
      "EndColumn": 68
    }
  },
  {
    "Id": "???",
    "Severity": 3,
    "Message": "CollectionErrors.ListInsteadOfIlist is declared as List which is not the correct way to declare to-many relationships in Realm. If you want to persist the collection, use the interface IList, otherwise annotate the property with the [Ignored] attribute.",
    "Location": {
      "Path": null,
      "StartLine": 40,
      "StartColumn": 9,
      "EndLine": 40,
      "EndColumn": 53
    }
  }
]
