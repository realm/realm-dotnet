import 'package:darttest/main.dart';
import 'package:test/test.dart';
import 'dart:io';

void main() {
  group('Test 1', () {
    test('Passing test', () {
      expect(1, equals(1));
    });

    group('Test 1.1', () {
      test('Failing test', () {
        expect(1, equals(2));
      });

      test('Exception in target unit', () {
        throwError();
      });
    });
  });

  group('Test 2', () {
    test('Exception in test', () {
      throw Exception('Some error');
    });
  });

  print('Hello from the test');
}
