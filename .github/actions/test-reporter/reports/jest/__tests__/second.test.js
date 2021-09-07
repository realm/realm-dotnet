test('Timeout test', async () => {
  await new Promise(resolve => setTimeout(resolve, 1000));
}, 1);

test.skip('Skipped test', () => {
  // do nothing
});
