import apiClient from '../api/apiClient';
import { API_ENDPOINTS } from '../api/endpoints';
import { mockSuccess } from '../mocks/mockHelper';
import { mockLearnerData, mockLessonCutQuestions, mockClassroomsDatabase } from '../mocks/mockLearner';

// Mặc định cho phép fallback mock data khi chạy local demo nếu backend chưa sẵn sàng
const USE_MOCK = import.meta.env.VITE_USE_MOCK !== 'false';

export const learnerService = {
  // -------------------------------------------------------------
  // NHÁNH A: CÂY KỸ NĂNG & LỘ TRÌNH HỌC (CURRICULUM)
  // -------------------------------------------------------------

  /**
   * Task A1: Lấy danh sách các chủ đề kỹ năng lớn (Domains)
   * GET /api/v1/domains
   */
  async getDomains() {
    try {
      const res = await apiClient.get(API_ENDPOINTS.CURRICULUM.DOMAINS);
      return res;
    } catch (err) {
      console.warn('Lỗi khi tải danh sách chủ đề, dùng fallback mock:', err);
      return mockSuccess(mockLearnerData.domains || []);
    }
  },

  /**
   * Task A2: Lấy bản đồ lộ trình học tập (Learning Path / Skill Tree)
   * GET /api/v1/path?domainId=...
   */
  async getLearningPath(domainId = null) {
    try {
      const res = await apiClient.get(API_ENDPOINTS.CURRICULUM.PATH(domainId));
      return res;
    } catch (err) {
      console.warn('Lỗi khi tải lộ trình học, dùng fallback mock:', err);
      return mockSuccess(mockLearnerData);
    }
  },

  /**
   * Task A1: Lấy chi tiết chặng kỹ năng kèm danh sách bài học
   * GET /api/v1/units/{id}
   */
  async getUnitDetail(unitId) {
    try {
      return await apiClient.get(API_ENDPOINTS.CURRICULUM.UNIT_DETAIL(unitId));
    } catch (err) {
      console.warn('Lỗi khi tải chi tiết chặng, dùng fallback mock:', err);
      return mockSuccess({});
    }
  },

  // -------------------------------------------------------------
  // NHÁNH C: BẢNG XẾP HẠNG TUẦN (WEEKLY LEADERBOARD)
  // -------------------------------------------------------------

  /**
   * Task C1: Lấy Bảng xếp hạng tuần hiện tại (Top 30 + vị trí của người dùng hiện tại)
   * GET /api/v1/leaderboard/current
   */
  async getWeeklyLeaderboard() {
    try {
      return await apiClient.get(API_ENDPOINTS.LEADERBOARD.CURRENT);
    } catch (err) {
      console.warn('Lỗi khi tải bảng xếp hạng, dùng fallback mock:', err);
      return mockSuccess(mockLearnerData.leaderboard || {});
    }
  },

  // -------------------------------------------------------------
  // PHÂN HỆ MEDIA & SUPABASE STORAGE
  // -------------------------------------------------------------

  /**
   * Tải lên ảnh đại diện và lưu vào Supabase Storage
   * POST /api/v1/media/upload/avatar
   */
  async uploadAvatar(file) {
    const formData = new FormData();
    formData.append('file', file);
    return await apiClient.post(API_ENDPOINTS.MEDIA.UPLOAD_AVATAR, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },

  /**
   * Tải lên hình ảnh/GIF câu hỏi tình huống (Curator/Admin)
   * POST /api/v1/media/upload/question-image
   */
  async uploadQuestionImage(file) {
    const formData = new FormData();
    formData.append('file', file);
    return await apiClient.post(API_ENDPOINTS.MEDIA.UPLOAD_QUESTION_IMAGE, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },

  async getDashboardData() {
    try {
      // Ưu tiên gọi API thật
      const [domainsRes, pathRes] = await Promise.all([
        apiClient.get(API_ENDPOINTS.CURRICULUM.DOMAINS).catch(() => null),
        apiClient.get(API_ENDPOINTS.CURRICULUM.PATH()).catch(() => null),
      ]);

      if (domainsRes?.data && pathRes?.data) {
        return {
          data: {
            ...mockLearnerData,
            domains: domainsRes.data,
            learningPath: pathRes.data,
            units: pathRes.data.units || [],
          },
        };
      }
    } catch {
      // ignore
    }
    return mockSuccess(mockLearnerData);
  },

  async getLessonDetail(lessonId) {
    if (USE_MOCK) {
      return mockSuccess({
        id: lessonId,
        title: 'Bài 2: Vết thương chảy máu & Cầm máu',
        unitTitle: 'Chặng 1: Xử lý Vết thương & Bỏng sinh hoạt',
        questions: mockLessonCutQuestions,
      });
    }
    try {
      return await apiClient.get(API_ENDPOINTS.LEARNER.LESSON_DETAIL(lessonId));
    } catch {
      return mockSuccess({
        id: lessonId,
        title: 'Bài 2: Vết thương chảy máu & Cầm máu',
        unitTitle: 'Chặng 1: Xử lý Vết thương & Bỏng sinh hoạt',
        questions: mockLessonCutQuestions,
      });
    }
  },

  // -------------------------------------------------------------
  // NHÁNH B: GAMEPLAY ENGINE & CHẤM ĐIỂM THỜI GIAN THỰC
  // -------------------------------------------------------------

  /**
   * Task B1: Khởi tạo phiên học mới cho Node bài học
   * POST /api/v1/lessons/{nodeId}/start
   */
  async startLesson(nodeId) {
    // Map mock IDs sang GUIDs đã seed trong BE nếu cần
    let targetNodeId = nodeId;
    const isGuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(nodeId);
    if (!isGuid) {
      if (nodeId === 'les_burn_1') targetNodeId = 'c1111111-1111-1111-1111-111111111111';
      else if (nodeId === 'les_cut_2') targetNodeId = 'c2222222-2222-2222-2222-222222222222';
      else targetNodeId = 'c1111111-1111-1111-1111-111111111111'; // Fallback node 1
    }

    try {
      return await apiClient.post(API_ENDPOINTS.GAMEPLAY.START_LESSON(targetNodeId));
    } catch (error) {
      console.warn('Gọi startLesson API thất bại, fallback sang mock data:', error);
      // Fallback cho chế độ offline nếu BE chưa có token
      return mockSuccess({
        sessionId: `mock-session-${Date.now()}`,
        nodeId: targetNodeId,
        nodeTitle: 'Bài 1.1: Bỏng nước sôi sinh hoạt & chập điện',
        heartsRemaining: 5,
        isPro: false,
        questions: mockLessonCutQuestions,
      });
    }
  },

  /**
   * Task B2: Nộp câu trả lời để Engine server chấm điểm
   * POST /api/v1/sessions/{id}/answer
   */
  async submitAnswer(sessionId, { questionId, submittedAnswer, timeSpentSeconds = 10 }, idempotencyKey = null) {
    if (sessionId?.startsWith('mock-session-')) {
      return mockSuccess({
        questionId,
        isCorrect: true,
        explanation: 'Lời giải thích mẫu: bạn đã thao tác chính xác!',
        heartsRemaining: 5,
        currentComboStreak: 1,
        xpEarned: 10,
        comboBonusXp: 0,
        isSessionFailed: false,
      });
    }

    const headers = {};
    if (idempotencyKey) {
      headers['Idempotency-Key'] = idempotencyKey;
    }

    return await apiClient.post(
      API_ENDPOINTS.GAMEPLAY.SUBMIT_ANSWER(sessionId),
      {
        questionId,
        submittedAnswer,
        timeSpentSeconds,
      },
      { headers }
    );
  },

  /**
   * Task B4: Tổng kết phiên học, nhận thưởng Perfect Lesson & cập nhật Streak
   * POST /api/v1/sessions/{id}/complete
   */
  async completeSession(sessionId) {
    if (sessionId?.startsWith('mock-session-')) {
      return mockSuccess({
        sessionId,
        status: 'completed',
        totalXpEarned: 35,
        heartsLost: 0,
        heartsRemaining: 5,
        maxComboStreak: 3,
        isPerfectLesson: true,
        perfectBonusXp: 5,
        streakInfo: {
          streak: 8,
          status: 'Increased',
          freezeConsumed: false,
          freezesRemaining: 1,
        },
      });
    }

    return await apiClient.post(API_ENDPOINTS.GAMEPLAY.COMPLETE_SESSION(sessionId));
  },

  // -------------------------------------------------------------
  // NHÁNH B: PHÒNG ÔN TẬP & HỒI TIM
  // -------------------------------------------------------------

  /**
   * Task B5: Lấy danh sách câu hỏi làm sai cần ôn tập
   * GET /api/v1/practice/wrong-questions
   */
  async getWrongQuestions() {
    try {
      return await apiClient.get(API_ENDPOINTS.PRACTICE.WRONG_QUESTIONS);
    } catch (error) {
      console.warn('Lấy wrong-questions từ API thất bại, fallback danh sách mẫu:', error);
      return mockSuccess([]);
    }
  },

  /**
   * Task B5: Nộp đáp án ôn tập để hồi tim và nhận XP
   * POST /api/v1/practice/answer
   */
  async submitPracticeAnswer({ questionId, submittedAnswer, timeSpentSeconds = 10 }) {
    return await apiClient.post(API_ENDPOINTS.PRACTICE.SUBMIT_ANSWER, {
      questionId,
      submittedAnswer,
      timeSpentSeconds,
    });
  },

  async claimQuest(questId) {
    if (USE_MOCK) {
      return mockSuccess({ success: true, message: 'Nhận thưởng thành công!' });
    }
    return apiClient.post('/api/learner/quests/claim', { questId });
  },

  async joinClassroom(code) {
    if (USE_MOCK) {
      const normalizedCode = (code || '').trim().toUpperCase();
      const targetClass = mockClassroomsDatabase[normalizedCode];

      if (targetClass) {
        mockLearnerData.classroom = {
          joined: true,
          classInfo: {
            code: targetClass.code,
            className: targetClass.className,
            school: targetClass.school,
            topic: targetClass.topic,
            teacher: targetClass.teacher,
            teacherRole: targetClass.teacherRole,
            teacherAvatar: targetClass.teacherAvatar,
            bannerGradient: targetClass.bannerGradient,
            totalStudents: targetClass.totalStudents,
            rankInClass: targetClass.rankInClass,
          },
          assignments: targetClass.assignments,
        };
        return mockSuccess({
          success: true,
          classroom: mockLearnerData.classroom,
          className: targetClass.className,
        });
      }

      const error = new Error('Mã lớp học không hợp lệ hoặc không tồn tại trong hệ thống liên kết!');
      throw error;
    }
    return apiClient.post('/api/learner/classroom/join', { code });
  },

  async leaveClassroom() {
    if (USE_MOCK) {
      mockLearnerData.classroom = {
        joined: false,
        classInfo: null,
        assignments: [],
      };
      return mockSuccess({ success: true, message: 'Đã rời lớp học thành công' });
    }
    return apiClient.post('/api/learner/classroom/leave');
  },
};
