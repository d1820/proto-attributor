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
  collectCoverageFrom: [
    'src/**/*.ts',
    '!src/test/**',
    '!src/**/*.test.ts',
    '!src/extension.ts',
  ],
  coverageDirectory: 'coverage',
  coverageThreshold: {
    global: {
      branches: 70,
      functions: 70,
      lines: 70,
      statements: 70,
    },
  },
}
