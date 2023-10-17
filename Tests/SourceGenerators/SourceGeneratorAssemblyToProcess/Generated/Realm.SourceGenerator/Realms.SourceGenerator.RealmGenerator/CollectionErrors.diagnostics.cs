[
  {
    "Id": "RLM020",
    "Severity": 3,
    "Message": "CollectionErrors.UnsupportetDictionaryKeyProp  is a Dictionary<int, string> but only string keys are currently supported by Realm.",
    "Location": {
      "StartLine": 27,
      "StartColumn": 9,
      "EndLine": 27,
      "EndColumn": 78
    }
  },
  {
    "Id": "RLM021",
    "Severity": 3,
    "Message": "CollectionErrors.SetOfEmbeddedObj is a Set<EmbeddedObject> which is not supported. Embedded objects are always unique which is why List<EmbeddedObject> already has Set semantics.",
    "Location": {
      "StartLine": 29,
      "StartColumn": 9,
      "EndLine": 29,
      "EndColumn": 59
    }
  },
  {
    "Id": "RLM019",
    "Severity": 3,
    "Message": "CollectionErrors.CollectionWithSetter has a setter but its type is a List which only supports getters.",
    "Location": {
      "StartLine": 31,
      "StartColumn": 9,
      "EndLine": 31,
      "EndColumn": 61
    }
  },
  {
    "Id": "RLM017",
    "Severity": 3,
    "Message": "CollectionErrors.CollectionOfRealmInteger is an List<RealmInteger> which is not supported.",
    "Location": {
      "StartLine": 33,
      "StartColumn": 9,
      "EndLine": 33,
      "EndColumn": 74
    }
  },
  {
    "Id": "RLM018",
    "Severity": 3,
    "Message": "CollectionErrors.CollectionOfUnsupportedType is an List but its generic type is System.DateTime which is not supported by Realm.",
    "Location": {
      "StartLine": 35,
      "StartColumn": 9,
      "EndLine": 35,
      "EndColumn": 68
    }
  },
  {
    "Id": "RLM022",
    "Severity": 3,
    "Message": "CollectionErrors.ListInsteadOfIList is declared as List which is not the correct way to declare to-many relationships in Realm. If you want to persist the collection, use the interface IList, otherwise annotate the property with the [Ignored] attribute.",
    "Location": {
      "StartLine": 37,
      "StartColumn": 9,
      "EndLine": 37,
      "EndColumn": 53
    }
  },
  {
    "Id": "RLM029",
    "Severity": 3,
    "Message": "CollectionErrors.CollectionWithInitializer is a collection with an initializer that is not supported. Realm collections are always initialized internally and initializing them to a non-null value is not supported.",
    "Location": {
      "StartLine": 39,
      "StartColumn": 9,
      "EndLine": 44,
      "EndColumn": 11
    }
  },
  {
    "Id": "RLM030",
    "Severity": 3,
    "Message": "CollectionErrors.CollectionWithCtorInitializer is a collection that is initialized in a constructor. Realm collections are always initialized internally and initializing them to a non-null value is not supported.",
    "Location": {
      "StartLine": 55,
      "StartColumn": 13,
      "EndLine": 55,
      "EndColumn": 63
    }
  }
]