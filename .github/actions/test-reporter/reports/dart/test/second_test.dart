import 'dart:async';
import 'package:test/test.dart';

void main() {
  test('Timeout test', () async {
    await Future.delayed(const Duration(seconds: 1));
  }, timeout: Timeout(Duration(microseconds: 1)));

  test('Skipped test', () {
    // do nothing
  }, skip: 'skipped test');
}
