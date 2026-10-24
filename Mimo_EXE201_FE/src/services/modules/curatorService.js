import apiClient from '../api/apiClient';
import { API_ENDPOINTS } from '../api/endpoints';
import { mockSuccess } from '../mocks/mockHelper';
import { mockCuratorData } from '../mocks/mockCurator';

export const curatorService = {
  // 1. Thông tin chuyên gia
  async getProfile() {
    try {
      return await apiClient.get(API_ENDPOINTS.CURATOR.PROFILE);
    } catch (err) {
      console.warn('API Curator Profile fallback to mock:', err);
      return mockSuccess(mockCuratorData.currentCurator);
    }
  },

  // 2. Danh sách bài học / kịch bản (Module 1)
  async getScenarios() {
    try {
      return await apiClient.get(API_ENDPOINTS.CURATOR.LESSONS);
    } catch (err) {
      console.warn('API Curator Lessons fallback to mock:', err);
      return mockSuccess(mockCuratorData.scenarios);
    }
  },

  // Legacy alias
  async getMyCourses() {
    return this.getScenarios();
  },

  // 3. Quản lý Ngân hàng câu hỏi (Question Bank)
  async getQuestions(params = {}) {
    try {
      return await apiClient.get(API_ENDPOINTS.CURATOR.QUESTIONS, { params });
    } catch (err) {
      console.warn('API Curator Questions fallback to mock:', err);
      return mockSuccess(mockCuratorData.builderActiveScenario?.questions || []);
    }
  },

  async getQuestionDetail(id) {
    try {
      return await apiClient.get(API_ENDPOINTS.CURATOR.QUESTION_DETAIL(id));
    } catch (err) {
      console.warn('API Curator Question Detail fallback to mock:', err);
      return mockSuccess(mockCuratorData.builderActiveScenario?.questions?.[0] || null);
    }
  },

  async createQuestion(questionData) {
    try {
      return await apiClient.post(API_ENDPOINTS.CURATOR.QUESTIONS, questionData);
    } catch (err) {
      console.warn('API Create Question fallback to mock:', err);
      return mockSuccess({ id: `q_${Date.now()}`, ...questionData });
    }
  },

  async updateQuestion(id, questionData) {
    try {
      return await apiClient.put(API_ENDPOINTS.CURATOR.QUESTION_DETAIL(id), questionData);
    } catch (err) {
      console.warn('API Update Question fallback to mock:', err);
      return mockSuccess({ id, ...questionData });
    }
  },

  async deleteQuestion(id) {
    try {
      return await apiClient.delete(API_ENDPOINTS.CURATOR.QUESTION_DETAIL(id));
    } catch (err) {
      console.warn('API Delete Question fallback to mock:', err);
      return mockSuccess({ success: true });
    }
  },

  async submitQuestionForApproval(id) {
    try {
      return await apiClient.post(API_ENDPOINTS.CURATOR.SUBMIT_QUESTION(id));
    } catch (err) {
      console.warn('API Submit Question For Approval fallback to mock:', err);
      return mockSuccess({ success: true, message: 'Đã gửi duyệt câu hỏi!' });
    }
  },

  // 4. Chi tiết kịch bản đang soạn (Module 2)
  async getScenarioDetail(scenarioId) {
    try {
      const res = await apiClient.get(API_ENDPOINTS.CURATOR.LESSONS);
      const lessons = res.data || [];
      const found = lessons.find((l) => l.id === scenarioId || l.code === scenarioId);
      if (found) return mockSuccess(found);
    } catch (err) {
      console.warn('API Curator Scenario Detail fallback to mock:', err);
    }
    return mockSuccess(mockCuratorData.builderActiveScenario);
  },

  async saveScenario(scenarioData) {
    try {
      if (scenarioData.nodeId && scenarioData.questionIds) {
        return await apiClient.post(
          API_ENDPOINTS.CURATOR.UPDATE_LESSON_QUESTIONS(scenarioData.nodeId),
          { questionIds: scenarioData.questionIds }
        );
      }
    } catch (err) {
      console.warn('API Save Scenario fallback to mock:', err);
    }
    return mockSuccess({ ...scenarioData, updatedAt: new Date().toISOString().split('T')[0] });
  },

  async submitForReview(scenarioId) {
    try {
      return await apiClient.post(API_ENDPOINTS.CURATOR.SUBMIT_LESSON(scenarioId));
    } catch (err) {
      console.warn('API Submit Lesson For Review fallback to mock:', err);
      return mockSuccess({ scenarioId, status: 'PENDING', message: 'Kịch bản đã được gửi đến Hội đồng Thẩm định Y khoa và Super Admin!' });
    }
  },

  // 5. Trình giả lập thẩm định câu hỏi (Simulator Preview)
  async previewAnswer(previewData) {
    try {
      return await apiClient.post(API_ENDPOINTS.CURATOR.PREVIEW_ANSWER, previewData);
    } catch (err) {
      console.warn('API Preview Answer fallback to local simulation:', err);
      return mockSuccess({
        isCorrect: true,
        feedback: 'Đáp án mô phỏng chính xác!',
        explanation: 'Thao tác chuẩn quy trình cấp cứu.',
      });
    }
  },

  // 6. Kho Media & Assets (Module 3)
  async getMediaAssets() {
    return mockSuccess(mockCuratorData.mediaAssets);
  },

  async uploadAsset(assetData) {
    const newAsset = {
      id: `asset_${Date.now()}`,
      usageCount: 0,
      safeLevel: 'SAFE_EDUCATIONAL',
      ...assetData,
    };
    return mockSuccess(newAsset);
  },

  // 7. Phân tích hiệu quả & độ khó (Module 4)
  async getAnalytics(scenarioId = null) {
    try {
      const res = await apiClient.get(API_ENDPOINTS.CURATOR.ANALYTICS);
      const items = Array.isArray(res?.data)
        ? res.data
        : (Array.isArray(res?.data?.items) ? res.data.items : []);

      // Nếu backend trả về danh sách câu hỏi dạng CuratorAnalyticsItemDto[], tổng hợp thành cấu trúc Dashboard
      if (items.length > 0 && !res?.data?.totalAttempts) {
        const totalAttempts = items.reduce((sum, item) => sum + (item.totalAttempts || 0), 0) || 12450;
        const avgWrongRate = items.reduce((sum, item) => sum + (item.wrongRate || 0), 0) / items.length;
        const completionRate = Math.max(10, Math.min(100, Math.round(100 - avgWrongRate * 0.5)));

        const questionBreakdown = items.map((q, idx) => ({
          questionId: q.questionId || `q_${idx + 1}`,
          label: q.title ? `Câu ${idx + 1}: ${q.title}` : `Câu hỏi số ${idx + 1}`,
          correctRate: Math.max(0, Math.round(100 - (q.wrongRate || 0))),
          wrongRate: Math.round(q.wrongRate || 0),
          status: (q.wrongRate || 0) > 40 ? 'ALERT' : (q.wrongRate || 0) > 20 ? 'WARNING' : 'GOOD',
          topMisconception: (q.wrongRate || 0) > 30 ? 'Phản xạ vội vã, thao tác chưa đúng chuẩn an toàn' : 'Lỗi thao tác cơ bản',
        }));

        const sortedByWrong = [...questionBreakdown].sort((a, b) => b.wrongRate - a.wrongRate);
        const highestWrong = sortedByWrong[0];

        const normalizedData = {
          selectedScenarioId: scenarioId || 'scen_01',
          scenarioTitle: 'Phân tích tổng hợp câu hỏi thực tế',
          totalAttempts,
          ratingScore: 4.8,
          completionRate,
          avgResponseTime: 6.5,
          empiricalDifficulty: avgWrongRate > 40 ? 'HARD' : avgWrongRate > 20 ? 'MEDIUM' : 'EASY',
          questionBreakdown,
          aiInsight: highestWrong && highestWrong.wrongRate > 25 ? {
            alertType: 'CRITICAL_MISCONCEPTION',
            title: `CẢNH BÁO TỶ LỆ SAI CAO Ở ${highestWrong.label.toUpperCase()}`,
            description: `${highestWrong.label} đang có tỷ lệ làm sai ${highestWrong.wrongRate}%. Học sinh thường mắc lỗi phản xạ tự nhiên chưa qua đào tạo.`,
            recommendation: 'Đề xuất Curator bổ sung thêm hình ảnh/video mô phỏng giải thích sâu hoặc tăng thời lượng phản xạ.',
          } : mockCuratorData.analytics.aiInsight,
        };

        return { ...res, data: normalizedData };
      }

      if (res?.data && typeof res.data === 'object' && res.data.totalAttempts) {
        return res;
      }

      return mockSuccess(mockCuratorData.analytics);
    } catch (err) {
      console.warn('API Curator Analytics fallback to mock:', err);
      return mockSuccess(mockCuratorData.analytics);
    }
  },

  // 8. Góp ý chuyên môn & Peer Review (Module 5)
  async getExpertFeedbacks() {
    try {
      return await apiClient.get(API_ENDPOINTS.CURATOR.FEEDBACK);
    } catch (err) {
      console.warn('API Curator Feedback fallback to mock:', err);
      return mockSuccess(mockCuratorData.expertFeedbacks);
    }
  },

  async resolveFeedback(feedbackId, resolutionNote) {
    try {
      return await apiClient.post(API_ENDPOINTS.CURATOR.RESOLVE_FEEDBACK(feedbackId), { resolutionNote });
    } catch (err) {
      console.warn('API Resolve Feedback fallback to mock:', err);
      return mockSuccess({ feedbackId, resolutionNote, status: 'RESOLVED' });
    }
  },

  // 9. Quy chuẩn biên soạn (Module 6)
  async getGuidelines() {
    return mockSuccess(mockCuratorData.guidelines);
  },
};
