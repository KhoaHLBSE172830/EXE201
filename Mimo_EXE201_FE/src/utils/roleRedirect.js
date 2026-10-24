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
    case '1':
    case 'ADMIN':
      return '/admin';
    case '2':
    case 'LEARNER':
    case 'STUDENT':
      return '/learn';
    case '3':
    case 'CURATOR':
      return '/studio';
    case '4':
    case 'ORGANIZATION':
    case '5':
    case 'EDUCATOR':
      return '/business';
    default:
      return '/learn';
  }
};
