const lib = require('../lib/main')

describe('Test 1', () => {
  test('Passing test', () => {
    expect(true).toBeTruthy()
  });

  describe('Test 1.1', () => {
    test('Failing test', () => {
      expect(false).toBeTruthy()
    });

    test('Exception in target unit', () => {
      lib.throwError();
    });
  });
});

describe('Test 2', () => {
  test('Exception in test', () => {
    throw new Error('Some error');
  });
});
