module.exports = {
  moduleFileExtensions: [
    'js',
    'ts',
  ],
  transform: {
    '^.+\\.(ts|tsx)$': [
      'ts-jest',
      {
      },
    ],
  },
  testPathIgnorePatterns: [
    '/node_modules/',
    '/src/test/',
    '/Sample/',
  ],
  testMatch: [
    '**/src/**/*.test.+(ts|js)',
  ],
  preset: 'ts-jest',
}
