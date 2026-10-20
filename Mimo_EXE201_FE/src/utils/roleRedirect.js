/**
 * Tiện ích điều hướng theo Role người dùng sau khi đăng nhập
 */
export const ROLES = {
  LEARNER: 'LEARNER',
  STUDENT: 'STUDENT',
  CURATOR: 'CURATOR',
  ORGANIZATION: 'ORGANIZATION',
  EDUCATOR: 'EDUCATOR',
  ADMIN: 'ADMIN',
};

export const getRedirectPathByRole = (role) => {
  const normalized = (role || '').toString().trim().toUpperCase();

  switch (normalized) {
    case 'LEARNER':
    case 'STUDENT':
      return '/learn';
    case 'CURATOR':
      return '/studio';
    case 'ORGANIZATION':
      return '/business';
    case 'EDUCATOR':
      return '/business';
    case 'ADMIN':
      return '/admin';
    default:
      return '/learn';
  }
};
