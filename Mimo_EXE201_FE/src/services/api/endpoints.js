/**
 * Danh sách Endpoints API của Backend (gom nhóm theo Role/Module)
 */
export const API_ENDPOINTS = {
  // Xác thực chung
  AUTH: {
    LOGIN: '/auth/login',
    REGISTER: '/auth/register',
    FORGOT_PASSWORD: '/auth/forgot-password',
    RESET_PASSWORD: '/auth/reset-password',
    ME: '/auth/me',
  },

  // 1. Phân hệ Học viên (Learner)
  LEARNER: {
    MAP_NODES: '/learner/map',
    LESSON_DETAIL: (id) => `/learner/lessons/${id}`,
    SUBMIT_QUIZ: (id) => `/learner/lessons/${id}/submit`,
    QUESTS: '/learner/quests',
    LEADERBOARD: '/learner/leaderboard',
    PROFILE: '/learner/profile',
  },

  // Phân hệ Học liệu & Cây kỹ năng (Curriculum & Learning Path - Nhánh A)
  CURRICULUM: {
    DOMAINS: '/domains',
    DOMAIN_DETAIL: (id) => `/domains/${id}`,
    PATH: (domainId) =>
      typeof domainId === 'string' && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(domainId)
        ? `/path?domainId=${domainId}`
        : '/path',
    UNIT_DETAIL: (id) => `/units/${id}`,
    LESSON_DETAIL: (nodeId) => `/lessons/${nodeId}`,
  },

  // Phân hệ Bảng xếp hạng tuần (Weekly Leaderboard - Nhánh C)
  LEADERBOARD: {
    CURRENT: '/leaderboard/current',
    SEED_MOCK: '/leaderboard/seed-mock',
  },

  // Phân hệ Đa phương tiện & Lưu trữ Cloud (Media & Supabase Storage)
  MEDIA: {
    UPLOAD_AVATAR: '/media/upload/avatar',
    UPLOAD_QUESTION_IMAGE: '/media/upload/question-image',
    UPLOAD: '/media/upload',
    DELETE: '/media',
  },

  // Phân hệ Gameplay Engine & Chấm điểm thời gian thực (Nhánh B)
  GAMEPLAY: {
    START_LESSON: (nodeId) => `/lessons/${nodeId}/start`,
    SUBMIT_ANSWER: (sessionId) => `/sessions/${sessionId}/answer`,
    COMPLETE_SESSION: (sessionId) => `/sessions/${sessionId}/complete`,
  },

  // Phân hệ Phòng ôn tập & Hồi tim (Nhánh B)
  PRACTICE: {
    WRONG_QUESTIONS: '/practice/wrong-questions',
    SUBMIT_ANSWER: '/practice/answer',
  },

  // 2. Phân hệ Tác giả soạn bài (Curator Studio)
  CURATOR: {
    COURSES: '/curator/courses',
    CREATE_COURSE: '/curator/courses',
    LESSON_EDITOR: (id) => `/curator/lessons/${id}`,
    SUBMIT_FOR_REVIEW: (id) => `/curator/lessons/${id}/submit`,
  },

  // 3. Phân hệ Nhà trường (Educator)
  EDUCATOR: {
    OVERVIEW: '/educator/overview',
    CLASSES: '/educator/classes',
    STUDENTS: (classId) => `/educator/classes/${classId}/students`,
    ASSIGNMENTS: '/educator/assignments',
  },

  // 4. Phân hệ Quản trị (Admin)
  ADMIN: {
    DASHBOARD: '/admin/analytics',
    USERS: '/admin/users',
    APPROVALS: '/admin/approvals',
    APPROVE_LESSON: (id) => `/admin/approvals/${id}/approve`,
    REJECT_LESSON: (id) => `/admin/approvals/${id}/reject`,
  },

  // 5. Cổng thanh toán (Payment VNPay & Subscription)
  PAYMENT: {
    CREATE_VNPAY_URL: '/payment/create-vnpay-url',
    VNPAY_RETURN: '/payment/vnpay-return',
    MOCK_ACTIVATE: '/payment/mock-activate',
    HISTORY: '/payment/history',
  },
};
