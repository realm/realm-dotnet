const assert = require('assert').strict;
const lib = require('../lib/main')

describe('Test 1', () => {
  it('Passing test', () => {
    assert.equal(true, true)
  });

  describe('Test 1.1', () => {
    it('Failing test', () => {
      assert.equal(false, true)
    });

    it('Exception in target unit', () => {
      lib.throwError();
    });
  });
});

describe('Test 2', () => {
  it('Exception in test', () => {
    throw new Error('Some error');
  });
});
