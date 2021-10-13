exports = function(...args) {
    return args.reduce((a,b) => a + b, 0);
};
