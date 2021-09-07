it('Timeout test', async function(done) {
  this.timeout(1);
  setTimeout(done, 1000);
});

it.skip('Skipped test', () => {
  // do nothing
});
