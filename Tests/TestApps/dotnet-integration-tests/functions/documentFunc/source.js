exports = function(first, second){
  return {
    intValue: parseInt(first.intValue + second.intValue),
    floatValue: parseFloat(first.floatValue + second.floatValue),
    stringValue: first.stringValue + second.stringValue,
    objectId: first.objectId,
    date: second.date,
    child: {
      intValue: parseInt(first.child.intValue + second.child.intValue)
    },
    arr: [ first.arr[0], second.arr[0] ]
  }
};